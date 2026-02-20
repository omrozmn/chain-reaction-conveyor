using UnityEngine;
using UnityEngine.UI;
using System;
using ChainReactionConveyor.Services;
using ChainReactionConveyor.Systems;

namespace ChainReactionConveyor.UI
{
    /// <summary>
    /// Event data for continue purchase
    /// </summary>
    public class ContinuePurchasedEvent
    {
        public int LevelId { get; set; }
        public int Cost { get; set; }
        public ContinueSource Source { get; set; }
    }

    /// <summary>
    /// Source of continue
    /// </summary>
    public enum ContinueSource
    {
        Ad,
        Currency,
        Lives
    }

    /// <summary>
    /// Manages continue system - allows player to continue after game over
    /// Uses IComponent pattern and EventBus for communication
    /// </summary>
    public partial class ContinueSystem : MonoBehaviour, IEventSubscriber
    {
        public static ContinueSystem Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject continuePanel;
        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button useCurrencyButton;
        [SerializeField] private Button useLifeButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Text costText;
        [SerializeField] private Text currencyAmountText;
        [SerializeField] private Text livesAmountText;
        [SerializeField] private Text timerText;
        [SerializeField] private CanvasGroup continueCanvasGroup;

        [Header("Continue Settings")]
        [SerializeField] private int baseCost = 100;
        [SerializeField] private int costIncreasePerContinue = 50;
        [SerializeField] private float continueTimerDuration = 10f;
        [SerializeField] private int maxContinuesPerLevel = 3;

        [Header("Currency Integration")]
        [SerializeField] private int playerCurrency = 500;
        [SerializeField] private int playerLives = 5;

        [Header("Ad Integration")]
        [SerializeField] private bool enableAdContinue = true;

        // State
        private int currentLevelId = 0;
        private int currentCost = 0;
        private int continuesUsed = 0;
        private float continueTimer = 0f;
        private bool isContinueActive = false;
        private int pendingContinues = 0;

        // Events
        public event Action OnContinueRequested;
        public event Action<ContinuePurchasedEvent> OnContinuePurchased;
        public event Action OnContinueSkipped;

        #region IEventSubscriber

        public void Subscribe()
        {
            EventBus.Instance.Subscribe<LevelFailEvent>(OnLevelFail);
            EventBus.Instance.Subscribe<LevelStartEvent>(OnLevelStart);
            EventBus.Instance.Subscribe<LevelCompleteEvent>(OnLevelComplete);
        }

        public void Unsubscribe()
        {
            EventBus.Instance.Clear<LevelFailEvent>();
            EventBus.Instance.Clear<LevelStartEvent>();
            EventBus.Instance.Clear<LevelCompleteEvent>();
        }

        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentCost = baseCost;
            InitializeButtons();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void InitializeButtons()
        {
            if (watchAdButton != null)
                watchAdButton.onClick.AddListener(OnWatchAdClicked);

            if (useCurrencyButton != null)
                useCurrencyButton.onClick.AddListener(OnUseCurrencyClicked);

            if (useLifeButton != null)
                useLifeButton.onClick.AddListener(OnUseLifeClicked);

            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
        }

        private void Start()
        {
            if (continuePanel != null)
                continuePanel.SetActive(false);

            UpdateCurrencyDisplay();
            UpdateLivesDisplay();
        }

        private void Update()
        {
            if (isContinueActive && continueTimer > 0f)
            {
                continueTimer -= Time.deltaTime;
                UpdateTimerDisplay();

                if (continueTimer <= 0f)
                {
                    TimeUp();
                }
            }
        }

        private void OnLevelFail(LevelFailEvent evt)
        {
            if (continuesUsed < maxContinuesPerLevel && pendingContinues <= 0)
            {
                ShowContinuePanel(evt.LevelId);
            }
        }

        private void OnLevelStart(LevelStartEvent evt)
        {
            ResetContinueState();
            currentLevelId = evt.LevelId;
        }

        private void OnLevelComplete(LevelCompleteEvent evt)
        {
            HideContinuePanel();
            ResetContinueState();
        }

        private void ShowContinuePanel(int levelId)
        {
            currentLevelId = levelId;
            currentCost = baseCost + (continuesUsed * costIncreasePerContinue);
            continueTimer = continueTimerDuration;
            isContinueActive = true;

            if (continuePanel != null)
                continuePanel.SetActive(true);

            UpdateCostDisplay();
            UpdateButtons();
            UpdateTimerDisplay();

            Debug.Log($"[ContinueSystem] Showing continue panel - Cost: {currentCost}");
            OnContinueRequested?.Invoke();
        }

        private void HideContinuePanel()
        {
            if (continuePanel != null)
                continuePanel.SetActive(false);
            isContinueActive = false;
            continueTimer = 0f;
        }

        private void UpdateCostDisplay()
        {
            if (costText != null)
                costText.text = $"{currentCost}";

            if (currencyAmountText != null)
                currencyAmountText.text = $"{playerCurrency}";

            if (livesAmountText != null)
                livesAmountText.text = $"{playerLives}";
        }

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(continueTimer).ToString();
                timerText.color = continueTimer <= 3f ? Color.red : Color.white;
            }
        }

        private void UpdateButtons()
        {
            // Update button states based on availability
            if (watchAdButton != null)
                watchAdButton.interactable = enableAdContinue;

            if (useCurrencyButton != null)
                useCurrencyButton.interactable = playerCurrency >= currentCost;

            if (useLifeButton != null)
                useLifeButton.interactable = playerLives > 0;
        }

        private void OnWatchAdClicked()
        {
            if (!enableAdContinue) return;

            Debug.Log("[ContinueSystem] Requesting ad for continue...");
            // In production, integrate with AdManager
            // For now, simulate ad completion
            SimulateAdComplete(ContinueSource.Ad);
        }

        private void OnUseCurrencyClicked()
        {
            if (playerCurrency < currentCost)
            {
                Debug.LogWarning("[ContinueSystem] Not enough currency!");
                return;
            }

            playerCurrency -= currentCost;
            UpdateCurrencyDisplay();
            ProcessContinue(ContinueSource.Currency);
        }

        private void OnUseLifeClicked()
        {
            if (playerLives <= 0)
            {
                Debug.LogWarning("[ContinueSystem] No lives remaining!");
                return;
            }

            playerLives--;
            UpdateLivesDisplay();
            ProcessContinue(ContinueSource.Lives);
        }

        private void OnSkipClicked()
        {
            HideContinuePanel();
            OnContinueSkipped?.Invoke();
            Debug.Log("[ContinueSystem] Continue skipped");
        }

        private void SimulateAdComplete(ContinueSource source)
        {
            // In production, this would be called from AdManager callback
            ProcessContinue(source);
        }

        private void ProcessContinue(ContinueSource source)
        {
            HideContinuePanel();
            continuesUsed++;
            pendingContinues--;

            var continueEvent = new ContinuePurchasedEvent
            {
                LevelId = currentLevelId,
                Cost = currentCost,
                Source = source
            };

            Debug.Log($"[ContinueSystem] Continue purchased - Level: {currentLevelId}, Source: {source}");
            OnContinuePurchased?.Invoke(continueEvent);

            // Publish event for GameManager to handle
            EventBus.Instance.Publish(new ContinueUsedEvent
            {
                LevelId = currentLevelId
            });
        }

        private void TimeUp()
        {
            Debug.Log("[ContinueSystem] Continue time up!");
            HideContinuePanel();
            OnContinueSkipped?.Invoke();
        }

        private void ResetContinueState()
        {
            continuesUsed = 0;
            currentCost = baseCost;
            continueTimer = 0f;
            isContinueActive = false;
            pendingContinues = 0;
        }

        #region Public API

        /// <summary>
        /// Request a continue (can be triggered externally)
        /// </summary>
        public void RequestContinue(int levelId)
        {
            if (continuesUsed >= maxContinuesPerLevel)
            {
                Debug.LogWarning("[ContinueSystem] Max continues reached!");
                return;
            }

            ShowContinuePanel(levelId);
        }

        /// <summary>
        /// Add currency (earned from gameplay)
        /// </summary>
        public void AddCurrency(int amount)
        {
            playerCurrency += amount;
            UpdateCurrencyDisplay();
            UpdateButtons();
        }

        /// <summary>
        /// Add lives
        /// </summary>
        public void AddLife(int amount = 1)
        {
            playerLives += amount;
            UpdateLivesDisplay();
            UpdateButtons();
        }

        /// <summary>
        /// Spend lives (cost for continue)
        /// </summary>
        public bool SpendLife()
        {
            if (playerLives > 0)
            {
                playerLives--;
                UpdateLivesDisplay();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get current currency amount
        /// </summary>
        public int GetCurrency() => playerCurrency;

        /// <summary>
        /// Get current lives
        /// </summary>
        public int GetLives() => playerLives;

        /// <summary>
        /// Check if can continue
        /// </summary>
        public bool CanContinue()
        {
            return continuesUsed < maxContinuesPerLevel && 
                   (playerCurrency >= currentCost || playerLives > 0 || enableAdContinue);
        }

        /// <summary>
        /// Get continues used this level
        /// </summary>
        public int GetContinuesUsed() => continuesUsed;

        /// <summary>
        /// Get remaining continues
        /// </summary>
        public int GetRemainingContinues() => maxContinuesPerLevel - continuesUsed;

        /// <summary>
        /// Force show continue panel (for testing)
        /// </summary>
        public void ForceShowContinue(int levelId)
        {
            ShowContinuePanel(levelId);
        }

        /// <summary>
        /// Set pending continues (e.g., from reward ads)
        /// </summary>
        public void SetPendingContinues(int count)
        {
            pendingContinues = count;
        }

        private void UpdateCurrencyDisplay()
        {
            if (currencyAmountText != null)
                currencyAmountText.text = $"{playerCurrency}";
        }

        private void UpdateLivesDisplay()
        {
            if (livesAmountText != null)
                livesAmountText.text = $"{playerLives}";
        }

        #endregion
    }
}
