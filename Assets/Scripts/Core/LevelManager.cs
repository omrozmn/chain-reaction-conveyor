using UnityEngine;
using System;
using System.Collections.Generic;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Core
{
    /// <summary>
    /// Manages level loading, state, and progression
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Data")]
        [SerializeField] private int currentLevelId = 1;
        [SerializeField] private LevelDef currentLevelDef;
        [SerializeField] private int seed;

        [Header("Difficulty")]
        [SerializeField] private int failCount = 0;
        [SerializeField] private bool isAdaptiveMode = false;

        public event Action<int> OnLevelStart;
        public event Action<int> OnLevelComplete;
        public event Action<string> OnLevelFail;
        public event Action<float> OnProgressChanged;

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
            // Generate initial seed
            GenerateNewSeed();
        }

        public void GenerateNewSeed()
        {
            seed = Guid.NewGuid().GetHashCode();
            Debug.Log($"[LevelManager] New seed generated: {seed}");
        }

        public void SetSeed(int newSeed)
        {
            seed = newSeed;
            Debug.Log($"[LevelManager] Seed set to: {seed}");
        }

        public int GetSeed() => seed;

        public void LoadLevel(int levelId)
        {
            currentLevelId = levelId;
            Debug.Log($"[LevelManager] Loading level {levelId}");

            // TODO: Load LevelDef from config
            // For now, create a default level
            currentLevelDef = CreateDefaultLevel(levelId);

            // Apply difficulty profile
            ApplyDifficultyProfile();

            OnLevelStart?.Invoke(levelId);
        }

        private LevelDef CreateDefaultLevel(int levelId)
        {
            return new LevelDef
            {
                levelId = levelId,
                seed = seed,
                boardWidth = 6,
                boardHeight = 8,
                minCluster = 3,
                spawnInterval = 1.5f,
                conveyorSpeed = 1.0f,
                targetProgress = 20,
                isSpike = false,
                isRecovery = false,
                isAnchor = false
            };
        }

        private void ApplyDifficultyProfile()
        {
            if (currentLevelDef == null) return;

            // Apply adaptive difficulty if fail count >= 3
            if (failCount >= 3 && isAdaptiveMode)
            {
                Debug.Log($"[LevelManager] Applying adaptive difficulty (fail count: {failCount})");
                currentLevelDef.spawnInterval *= 1.1f; // Slow down spawn
                currentLevelDef.conveyorSpeed *= 0.9f; // Slower conveyor
            }
        }

        public void CompleteLevel()
        {
            Debug.Log($"[LevelManager] Level {currentLevelId} completed!");
            failCount = 0; // Reset fail count on win
            OnLevelComplete?.Invoke(currentLevelId);
        }

        public void FailLevel(string reason)
        {
            failCount++;
            Debug.Log($"[LevelManager] Level {currentLevelId} failed! Reason: {reason}, Fail count: {failCount}");
            OnLevelFail?.Invoke(reason);
        }

        public void UpdateProgress(float progress)
        {
            OnProgressChanged?.Invoke(progress);
        }

        public LevelDef GetCurrentLevel() => currentLevelDef;
        public int GetCurrentLevelId() => currentLevelId;
        public int GetFailCount() => failCount;

        public void SetAdaptiveMode(bool enabled)
        {
            isAdaptiveMode = enabled;
        }

        public void ResetFailCount()
        {
            failCount = 0;
        }
    }
}
