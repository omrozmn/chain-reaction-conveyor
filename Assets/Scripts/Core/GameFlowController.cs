using UnityEngine;
using System;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Core
{
    /// <summary>
    /// Controls game flow: start, win, fail, continue, restart
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        public static GameFlowController Instance { get; private set; }

        [Header("Flow Settings")]
        [SerializeField] private bool canContinue = true;
        [SerializeField] private int continueUsedCount = 0;
        [SerializeField] private int maxContinuePerLevel = 1;

        [Header("Monetization")]
        [SerializeField] private bool continueAvailable = true;
        [SerializeField] private bool rewardedAdReady = false;

        public event Action OnGameStart;
        public event Action OnGameWin;
        public event Action OnGameFail;
        public event Action OnContinueRequested;
        public event Action OnContinueUsed;
        public event Action OnRestart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to LevelManager events
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelComplete += HandleLevelComplete;
                LevelManager.Instance.OnLevelFail += HandleLevelFail;
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelComplete -= HandleLevelComplete;
                LevelManager.Instance.OnLevelFail -= HandleLevelFail;
            }
        }

        public void StartGameplay()
        {
            Debug.Log("[GameFlowController] Starting gameplay");
            continueUsedCount = 0;
            continueAvailable = canContinue;
            OnGameStart?.Invoke();
        }

        public void HandleWin()
        {
            Debug.Log("[GameFlowController] Win!");
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteLevel();
            }
            OnGameWin?.Invoke();
        }

        public void HandleFail()
        {
            Debug.Log("[GameFlowController] Fail!");
            OnGameFail?.Invoke();
        }

        public void RequestContinue()
        {
            if (!continueAvailable || continueUsedCount >= maxContinuePerLevel)
            {
                Debug.Log("[GameFlowController] Continue not available");
                return;
            }

            Debug.Log("[GameFlowController] Continue requested");
            OnContinueRequested?.Invoke();
        }

        public void UseContinue()
        {
            if (!continueAvailable || continueUsedCount >= maxContinuePerLevel)
            {
                Debug.Log("[GameFlowController] Cannot use continue");
                return;
            }

            continueUsedCount++;
            ApplyContinueBias();
            Debug.Log($"[GameFlowController] Continue used ({continueUsedCount}/{maxContinuePerLevel})");
            OnContinueUsed?.Invoke();

            // Resume gameplay
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(AppState.Playing);
            }
        }

        private void ApplyContinueBias()
        {
            // Apply bias to help player win after continue
            // This is part of NearMissEngine integration
            Debug.Log("[GameFlowController] Applying continue bias");
        }

        public void RestartLevel()
        {
            Debug.Log("[GameFlowController] Restarting level");
            continueUsedCount = 0;
            continueAvailable = canContinue;

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(LevelManager.Instance.GetCurrentLevelId());
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(AppState.Playing);
            }

            OnRestart?.Invoke();
        }

        public void NextLevel()
        {
            int nextLevel = LevelManager.Instance.GetCurrentLevelId() + 1;
            Debug.Log($"[GameFlowController] Loading next level: {nextLevel}");
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(nextLevel);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(AppState.Loading);
                GameManager.Instance.SetState(AppState.Playing);
            }
        }

        public bool CanContinue() => continueAvailable && continueUsedCount < maxContinuePerLevel;
        public int GetContinueUsedCount() => continueUsedCount;
        public int GetMaxContinue() => maxContinuePerLevel;

        public void SetRewardedAdReady(bool ready)
        {
            rewardedAdReady = ready;
        }

        private void HandleLevelComplete(int levelId)
        {
            Debug.Log($"[GameFlowController] Level {levelId} completed");
        }

        private void HandleLevelFail(string reason)
        {
            Debug.Log($"[GameFlowController] Level failed: {reason}");
        }
    }
}
