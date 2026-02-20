using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace ChainReactionConveyor.UI
{
    /// <summary>
    /// Main UI Controller - manages all UI screens and elements
    /// </summary>
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("UI Screens")]
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject pauseScreen;

        [Header("HUD Elements")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text comboText;
        [SerializeField] private Image slowTimerFill;
        
        [Header("Game Over Elements")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text starsText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Booster UI")]
        [SerializeField] private Button swapButton;
        [SerializeField] private Button bombButton;
        [SerializeField] private Button slowButton;
        [SerializeField] private Text swapCountText;
        [SerializeField] private Text bombCountText;
        [SerializeField] private Text slowCountText;

        // Screen states
        private enum ScreenState { MainMenu, HUD, GameOver, Paused }
        private ScreenState currentState = ScreenState.MainMenu;

        // Events
        public event Action OnPlayClicked;
        public event Action OnRestartClicked;
        public event Action OnMainMenuClicked;
        public event Action OnPauseClicked;
        public event Action OnResumeClicked;
        public event Action<BoosterType> OnBoosterButtonClicked;

        private bool isPaused = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeButtons();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void InitializeButtons()
        {
            // These would be connected in Inspector or dynamically
            if (restartButton != null)
                restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
            
            if (swapButton != null)
                swapButton.onClick.AddListener(() => OnBoosterButtonClicked?.Invoke(BoosterType.Swap));
            
            if (bombButton != null)
                bombButton.onClick.AddListener(() => OnBoosterButtonClicked?.Invoke(BoosterType.Bomb));
            
            if (slowButton != null)
                slowButton.onClick.AddListener(() => OnBoosterButtonClicked?.Invoke(BoosterType.Slow));
        }

        #region Screen Management

        public void ShowMainMenu()
        {
            HideAllScreens();
            if (mainMenuScreen != null)
                mainMenuScreen.SetActive(true);
            
            currentState = ScreenState.MainMenu;
            Debug.Log("[UIController] Showing Main Menu");
        }

        public void ShowHUD()
        {
            HideAllScreens();
            if (hudScreen != null)
                hudScreen.SetActive(true);
            
            currentState = ScreenState.HUD;
            Debug.Log("[UIController] Showing HUD");
        }

        public void ShowGameOver(int score, int highScore, int stars)
        {
            HideAllScreens();
            if (gameOverScreen != null)
                gameOverScreen.SetActive(true);
            
            // Update game over text
            if (finalScoreText != null)
                finalScoreText.text = $"Score: {score}";
            
            if (highScoreText != null)
                highScoreText.text = $"Best: {highScore}";
            
            if (starsText != null)
                starsText.text = GetStarsString(stars);
            
            currentState = ScreenState.GameOver;
            Debug.Log($"[UIController] Showing Game Over - Score: {score}, Stars: {stars}");
        }

        public void ShowPause()
        {
            if (currentState != ScreenState.HUD) return;
            
            isPaused = true;
            if (pauseScreen != null)
                pauseScreen.SetActive(true);
            
            currentState = ScreenState.Paused;
            Debug.Log("[UIController] Showing Pause");
        }

        public void HidePause()
        {
            if (pauseScreen != null)
                pauseScreen.SetActive(false);
            
            isPaused = false;
            currentState = ScreenState.HUD;
            Debug.Log("[UIController] Hiding Pause");
        }

        private void HideAllScreens()
        {
            if (mainMenuScreen != null) mainMenuScreen.SetActive(false);
            if (hudScreen != null) hudScreen.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(false);
            if (pauseScreen != null) pauseScreen.SetActive(false);
        }

        #endregion

        #region HUD Updates

        public void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        public void UpdateLevel(int level)
        {
            if (levelText != null)
                levelText.text = $"Level {level}";
        }

        public void UpdateCombo(int combo)
        {
            if (comboText != null)
            {
                comboText.text = combo > 1 ? $"Combo x{combo}" : "";
                comboText.gameObject.SetActive(combo > 1);
            }
        }

        public void UpdateBoosterCounts(int swapCount, int bombCount, int slowCount)
        {
            if (swapCountText != null)
                swapCountText.text = swapCount.ToString();
            
            if (bombCountText != null)
                bombCountText.text = bombCount.ToString();
            
            if (slowCountText != null)
                slowCountText.text = slowCount.ToString();

            // Enable/disable buttons based on availability
            if (swapButton != null)
                swapButton.interactable = swapCount > 0;
            
            if (bombButton != null)
                bombButton.interactable = bombCount > 0;
            
            if (slowButton != null)
                slowButton.interactable = slowCount > 0;
        }

        public void UpdateSlowTimer(float progress)
        {
            if (slowTimerFill != null)
            {
                slowTimerFill.fillAmount = progress;
                slowTimerFill.gameObject.SetActive(progress > 0);
            }
        }

        #endregion

        #region Helper Methods

        private string GetStarsString(int stars)
        {
            string result = "";
            for (int i = 0; i < 3; i++)
            {
                result += i < stars ? "★" : "☆";
            }
            return result;
        }

        public bool IsPaused() => isPaused;

        public void Play()
        {
            ShowHUD();
            OnPlayClicked?.Invoke();
        }

        public void Restart()
        {
            HideAllScreens();
            ShowHUD();
            OnRestartClicked?.Invoke();
        }

        public void GoToMainMenu()
        {
            HideAllScreens();
            ShowMainMenu();
            OnMainMenuClicked?.Invoke();
        }

        public void TogglePause()
        {
            if (isPaused)
                HidePause();
            else
                ShowPause();
            
            OnPauseClicked?.Invoke();
        }

        #endregion
    }
}
