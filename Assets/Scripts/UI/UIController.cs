using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using ChainReactionConveyor.Services;
using ChainReactionConveyor.Systems;

namespace ChainReactionConveyor.UI
{
    /// <summary>
    /// Main UI Controller - manages all UI screens and elements
    /// Uses IComponent pattern and EventBus for communication
    /// </summary>
    public class UIController : MonoBehaviour, IEventSubscriber
    {
        public static UIController Instance { get; private set; }

        [Header("UI Screens")]
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject hudScreen;
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject settingsScreen;

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

        [Header("Settings UI")]
        [SerializeField] private Button themeNextButton;
        [SerializeField] private Button themePrevButton;
        [SerializeField] private Text themeNameText;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle vibrationToggle;

        [Header("Theme Integration")]
        [SerializeField] private bool applyThemeOnStart = true;

        #region IEventSubscriber

        public void Subscribe()
        {
            EventBus.Instance.Subscribe<LevelStartEvent>(OnLevelStart);
            EventBus.Instance.Subscribe<LevelCompleteEvent>(OnLevelComplete);
            EventBus.Instance.Subscribe<LevelFailEvent>(OnLevelFail);
            EventBus.Instance.Subscribe<BoosterInventoryChangedEvent>(OnBoosterInventoryChanged);
            EventBus.Instance.Subscribe<ThemeChangedEvent>(OnThemeChanged);
            EventBus.Instance.Subscribe<ChainResolvedEvent>(OnChainResolved);
        }

        public void Unsubscribe()
        {
            EventBus.Instance.Clear<LevelStartEvent>();
            EventBus.Instance.Clear<LevelCompleteEvent>();
            EventBus.Instance.Clear<LevelFailEvent>();
            EventBus.Instance.Clear<BoosterInventoryChangedEvent>();
            EventBus.Instance.Clear<ThemeChangedEvent>();
            EventBus.Instance.Clear<ChainResolvedEvent>();
        }

        #endregion

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
        public event Action OnSettingsClicked;

        private bool isPaused = false;
        private bool isSettingsOpen = false;

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
            SubscribeToThemeEvents();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void SubscribeToThemeEvents()
        {
            if (ColorsManager.Instance != null)
            {
                ColorsManager.Instance.OnThemeChanged += OnThemeChangedHandler;
            }
        }

        private void OnThemeChangedHandler(Systems.ColorTheme theme)
        {
            UpdateThemeDisplay();
        }

        private void UpdateThemeDisplay()
        {
            if (themeNameText != null && ColorsManager.Instance != null)
            {
                themeNameText.text = ColorsManager.Instance.CurrentTheme?.themeName ?? "Default";
            }
        }

        private void Start()
        {
            ShowMainMenu();
            
            if (applyThemeOnStart && ColorsManager.Instance != null)
            {
                ColorsManager.Instance.ApplyThemeToUI();
            }
            
            InitializeSettingsButtons();
            UpdateThemeDisplay();
        }

        private void InitializeSettingsButtons()
        {
            if (themeNextButton != null)
                themeNextButton.onClick.AddListener(() => {
                    ColorsManager.Instance?.NextTheme();
                    OnSettingsClicked?.Invoke();
                });
            
            if (themePrevButton != null)
                themePrevButton.onClick.AddListener(() => {
                    ColorsManager.Instance?.PreviousTheme();
                    OnSettingsClicked?.Invoke();
                });
            
            if (volumeSlider != null)
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            
            if (soundToggle != null)
                soundToggle.onValueChanged.AddListener(OnSoundToggle);
            
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(OnVibrationToggle);
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }

        private void OnSoundToggle(bool enabled)
        {
            AudioListener.pause = !enabled;
        }

        private void OnVibrationToggle(bool enabled)
        {
            // Handheld.Vibrate() can be called when available
            Debug.Log("[UIController] Vibration " + (enabled ? "enabled" : "disabled"));
        }

        public void OpenSettings()
        {
            if (settingsScreen != null)
            {
                settingsScreen.SetActive(true);
                isSettingsOpen = true;
            }
        }

        public void CloseSettings()
        {
            if (settingsScreen != null)
            {
                settingsScreen.SetActive(false);
                isSettingsOpen = false;
            }
        }

        public void ToggleSettings()
        {
            if (isSettingsOpen)
                CloseSettings();
            else
                OpenSettings();
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

        #region EventBus Handlers

        private void OnLevelStart(LevelStartEvent evt)
        {
            ShowHUD();
            UpdateLevel(evt.LevelId);
            UpdateScore(0);
            UpdateBoosterCounts(
                BoosterManager.Instance?.GetCharges(BoosterType.Swap) ?? 0,
                BoosterManager.Instance?.GetCharges(BoosterType.Bomb) ?? 0,
                BoosterManager.Instance?.GetCharges(BoosterType.Slow) ?? 0
            );
        }

        private void OnLevelComplete(LevelCompleteEvent evt)
        {
            ShowGameOver(evt.Score, PlayerPrefs.GetInt("HighScore", 0), CalculateStars(evt.Score));
        }

        private void OnLevelFail(LevelFailEvent evt)
        {
            ShowGameOver(PlayerPrefs.GetInt("CurrentScore", 0), PlayerPrefs.GetInt("HighScore", 0), 0);
        }

        private void OnBoosterInventoryChanged(BoosterInventoryChangedEvent evt)
        {
            UpdateBoosterCounts(
                BoosterManager.Instance?.GetCharges(BoosterType.Swap) ?? 0,
                BoosterManager.Instance?.GetCharges(BoosterType.Bomb) ?? 0,
                BoosterManager.Instance?.GetCharges(BoosterType.Slow) ?? 0
            );
        }

        private void OnThemeChanged(ThemeChangedEvent evt)
        {
            UpdateThemeDisplay();
            ColorsManager.Instance.ApplyThemeToUI();
        }

        private void OnChainResolved(ChainResolvedEvent evt)
        {
            // Could update combo display here
            UpdateCombo(evt.ChainDepth);
        }

        #endregion

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
            
            // Also publish to EventBus
            EventBus.Instance.Publish(new ScoreUpdatedEvent { Score = score });
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

        private int CalculateStars(int score)
        {
            if (score >= 1000) return 3;
            if (score >= 500) return 2;
            if (score >= 100) return 1;
            return 0;
        }

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

    #region Additional Events

    public struct ScoreUpdatedEvent
    {
        public int Score;
    }

    #endregion
}
