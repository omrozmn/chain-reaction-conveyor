using UnityEngine;
using UnityEngine.UI;
using System;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.UI
{
    /// <summary>
    /// UI Bar displaying current chain combo progress
    /// </summary>
    public class ComboBar : MonoBehaviour, IEventSubscriber
    {
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Text comboText;
        [SerializeField] private Text multiplierText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Combo Settings")]
        [SerializeField] private float comboTimeout = 3f;
        [SerializeField] private float fillSpeed = 2f;
        [SerializeField] private int maxCombo = 10;

        [Header("Visual Feedback")]
        [SerializeField] private Color lowComboColor = Color.white;
        [SerializeField] private Color highComboColor = Color.yellow;
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private float pulseDuration = 0.2f;

        // State
        private int currentCombo = 0;
        private float comboTimer = 0f;
        private float currentFillAmount = 0f;
        private bool isPulsing = false;

        // Events
        public event Action<int> OnComboUpdated;
        public event Action OnComboMaxed;

        #region IEventSubscriber

        public void Subscribe()
        {
            EventBus.Instance.Subscribe<ChainResolvedEvent>(OnChainResolved);
            EventBus.Instance.Subscribe<LevelStartEvent>(OnLevelStart);
            EventBus.Instance.Subscribe<LevelFailEvent>(OnLevelFail);
            EventBus.Instance.Subscribe<LevelCompleteEvent>(OnLevelComplete);
        }

        public void Unsubscribe()
        {
            EventBus.Instance.Clear<ChainResolvedEvent>();
            EventBus.Instance.Clear<LevelStartEvent>();
            EventBus.Instance.Clear<LevelFailEvent>();
            EventBus.Instance.Clear<LevelCompleteEvent>();
        }

        #endregion

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;

            UpdateComboDisplay();
        }

        private void Update()
        {
            if (currentCombo > 0)
            {
                comboTimer -= Time.deltaTime;
                UpdateFillAmount();

                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }
        }

        private void UpdateFillAmount()
        {
            if (fillImage != null)
            {
                // Calculate fill based on remaining time
                float targetFill = comboTimer / comboTimeout;
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFill, Time.deltaTime * fillSpeed);
                fillImage.fillAmount = currentFillAmount;
            }
        }

        private void OnChainResolved(ChainResolvedEvent evt)
        {
            IncrementCombo(evt.ChainDepth);
        }

        private void OnLevelStart(LevelStartEvent evt)
        {
            ResetCombo();
        }

        private void OnLevelFail(LevelFailEvent evt)
        {
            // Keep combo visible briefly
            Invoke(nameof(HideCombo), 1f);
        }

        private void OnLevelComplete(LevelCompleteEvent evt)
        {
            HideCombo();
        }

        private void IncrementCombo(int chainDepth)
        {
            currentCombo = Mathf.Min(currentCombo + chainDepth, maxCombo);
            comboTimer = comboTimeout;

            UpdateComboDisplay();
            OnComboUpdated?.Invoke(currentCombo);

            if (currentCombo >= maxCombo)
            {
                OnComboMaxed?.Invoke();
            }

            PulseAnimation();
            ShowCombo();
        }

        private void ResetCombo()
        {
            currentCombo = 0;
            comboTimer = 0f;
            currentFillAmount = 0f;
            UpdateComboDisplay();
            HideCombo();
        }

        private void UpdateComboDisplay()
        {
            if (comboText != null)
            {
                comboText.text = currentCombo > 0 ? $"x{currentCombo}" : "";
            }

            if (multiplierText != null)
            {
                // Calculate score multiplier based on combo
                float multiplier = 1f + (currentCombo * 0.1f);
                multiplierText.text = currentCombo > 1 ? $"{multiplier:F1}x" : "";
            }

            // Update color based on combo level
            if (fillImage != null)
            {
                float t = (float)currentCombo / maxCombo;
                fillImage.color = Color.Lerp(lowComboColor, highComboColor, t);
            }
        }

        private void ShowCombo()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void HideCombo()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void PulseAnimation()
        {
            if (!isPulsing && comboText != null)
            {
                StartCoroutine(PulseCoroutine());
            }
        }

        private System.Collections.IEnumerator PulseCoroutine()
        {
            isPulsing = true;
            Vector3 originalScale = comboText.transform.localScale;
            Vector3 targetScale = originalScale * pulseScale;

            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                comboText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            comboText.transform.localScale = originalScale;
            isPulsing = false;
        }

        /// <summary>
        /// Manually add combo (for external triggers like near-miss)
        /// </summary>
        public void AddCombo(int amount)
        {
            currentCombo = Mathf.Min(currentCombo + amount, maxCombo);
            comboTimer = comboTimeout;
            UpdateComboDisplay();
            OnComboUpdated?.Invoke(currentCombo);
            PulseAnimation();
            ShowCombo();
        }

        /// <summary>
        /// Get current combo count
        /// </summary>
        public int GetCurrentCombo() => currentCombo;

        /// <summary>
        /// Check if combo is active
        /// </summary>
        public bool IsComboActive() => currentCombo > 0 && comboTimer > 0f;

        /// <summary>
        /// Get current multiplier
        /// </summary>
        public float GetMultiplier()
        {
            return 1f + (currentCombo * 0.1f);
        }
    }
}
