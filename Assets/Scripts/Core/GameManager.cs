using UnityEngine;
using System;

namespace ChainReactionConveyor.Core
{
    /// <summary>
    /// Main game manager - orchestrates all core systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameFlowController gameFlowController;

        [Header("App State")]
        public AppState CurrentState { get; private set; } = AppState.Boot;

        public event Action<AppState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeCoreSystems();
            SetState(AppState.MainMenu);
        }

        private void InitializeCoreSystems()
        {
            // Initialize LevelManager
            if (levelManager == null)
            {
                levelManager = gameObject.AddComponent<LevelManager>();
            }

            // Initialize GameFlowController
            if (gameFlowController == null)
            {
                gameFlowController = gameObject.AddComponent<GameFlowController>();
            }

            Debug.Log("[GameManager] Core systems initialized");
        }

        public void SetState(AppState newState)
        {
            if (CurrentState == newState) return;

            Debug.Log($"[GameManager] State changed: {CurrentState} → {newState}");
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            // Handle state transitions
            switch (newState)
            {
                case AppState.MainMenu:
                    break;
                case AppState.Loading:
                    break;
                case AppState.Playing:
                    gameFlowController.StartGameplay();
                    break;
                case AppState.Paused:
                    Time.timeScale = 0f;
                    break;
                case AppState.Win:
                    gameFlowController.HandleWin();
                    break;
                case AppState.Fail:
                    gameFlowController.HandleFail();
                    break;
            }
        }

        public void Resume()
        {
            if (CurrentState == AppState.Paused)
            {
                Time.timeScale = 1f;
                SetState(AppState.Playing);
            }
        }

        public LevelManager GetLevelManager() => levelManager;
        public GameFlowController GetGameFlowController() => gameFlowController;
    }
}
