using UnityEngine;
using UnityEngine.UI;
using System;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Continue system - handles game over continue options
    /// </summary>
    public class ContinueSystem : MonoBehaviour
    {
        public static ContinueSystem Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject continuePanel;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueCostText;
        [SerializeField] private Text continueCountText;
        [SerializeField] private GameObject noContinuesText;

        [Header("Settings")]
        [SerializeField] private int maxContinuesPerLevel = 3;
        [SerializeField] private int continueCost = 100; // gems
        [SerializeField] private bool useStarterPack = true; // Free continues for new players
        [SerializeField] private int starterContinues = 5;

        [Header("Audio")]
        [SerializeField] private AudioClip continueSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioSource audioSource;

        // State
        private int currentContinuesUsed = 0;
        private bool isContinueAvailable = false;
        private int currentLevelId = -1;

        // Events
        public event Action<int> OnContinueUsed;      // levelId
        public event Action<int, string> OnContinueFailed; // levelId, reason
        public event Action OnContinuePanelShown;
        public event Action OnContinuePanelHidden;

        public int ContinuesRemaining => maxContinuesPerLevel - currentContinuesUsed;
        public bool CanContinue => isContinueAvailable && currentContinuesUsed < maxContinuesPerLevel;

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
            // Subscribe to game events
            EventBus.Instance.Subscribe<LevelStartEvent>(OnLevelStart);
            EventBus.Instance.Subscribe<LevelFailEvent>(OnLevelFail);

            // Setup button listener
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            }

            // Hide panel initially
            HideContinuePanel();
        }

        private void OnDestroy()
        {
            EventBus.Instance.Unsubscribe<LevelStartEvent>(OnLevelStart);
            EventBus.Instance.Unsubscribe<LevelFailEvent>(OnLevelFail);
        }

        private void OnLevelStart(LevelStartEvent e)
        {
            ResetContinues();
            currentLevelId = e.LevelId;
            HideContinuePanel();
        }

        private void OnLevelFail(LevelFailEvent e)
        {
            // Check if continue is available and player has resources
            CheckContinueAvailability();
            
            if (CanContinue)
            {
                ShowContinuePanel();
                OnContinuePanelShown?.Invoke();
            }
            else
            {
                HideContinuePanel();
            }
        }

        private void CheckContinueAvailability()
        {
            // Check gem balance or starter pack
            int playerGems = PlayerPrefs.GetInt("PlayerGems", 0);
            
            // Check if using starter pack (free continues for new players)
            if (useStarterPack)
            {
                int starterUsed = PlayerPrefs.GetInt("StarterContinuesUsed", 0);
                int totalStarter = starterContinues;
                
                if (starterUsed < totalStarter)
                {
                    isContinueAvailable = true;
                    return;
                }
            }

            // Check gem balance
            isContinueAvailable = playerGems >= continueCost;
        }

        private void ShowContinuePanel()
        {
            if (continuePanel == null) return;

            continuePanel.SetActive(true);
            
            // Update UI
            UpdateContinueUI();
        }

        private void HideContinuePanel()
        {
            if (continuePanel != null)
            {
                continuePanel.SetActive(false);
                OnContinuePanelHidden?.Invoke();
            }
        }

        private void UpdateContinueUI()
        {
            int remaining = ContinuesRemaining;
            
            // Update continue count
            if (continueCountText != null)
            {
                continueCountText.text = $"{remaining}/{maxContinuesPerLevel}";
            }

            // Check if any continues left
            bool hasContinues = remaining > 0;
            
            if (continueButton != null)
            {
                continueButton.interactable = hasContinues && isContinueAvailable;
            }

            // Show/hide no continues text
            if (noContinuesText != null)
            {
                noContinuesText.SetActive(!hasContinues);
            }

            // Update cost text
            if (continueCostText != null)
            {
                if (useStarterPack)
                {
                    int starterUsed = PlayerPrefs.GetInt("StarterContinuesUsed", 0);
                    int remainingStarter = starterContinues - starterUsed;
                    
                    if (remainingStarter > 0)
                    {
                        continueCostText.text = $"FREE ({remainingStarter} left)";
                    }
                    else
                    {
                        continueCostText.text = $"{continueCost} Gems";
                    }
                }
                else
                {
                    continueCostText.text = $"{continueCost} Gems";
                }
            }
        }

        private void OnContinueButtonClicked()
        {
            if (!CanContinue)
            {
                OnContinueFailed?.Invoke(currentLevelId, "No continues available");
                PlaySound(failSound);
                return;
            }

            // Consume continue
            bool success = ConsumeContinue();
            
            if (success)
            {
                PlaySound(continueSound);
                OnContinueUsed?.Invoke(currentLevelId);
                EventBus.Instance.Publish(new ContinueUsedEvent { LevelId = currentLevelId });
                
                // Hide panel and restart level
                HideContinuePanel();
                
                // Notify game to restart level
                NotifyGameToRestart();
            }
            else
            {
                OnContinueFailed?.Invoke(currentLevelId, "Failed to consume continue");
                PlaySound(failSound);
            }
        }

        private bool ConsumeContinue()
        {
            // Use starter pack if available
            if (useStarterPack)
            {
                int starterUsed = PlayerPrefs.GetInt("StarterContinuesUsed", 0);
                if (starterUsed < starterContinues)
                {
                    PlayerPrefs.SetInt("StarterContinuesUsed", starterUsed + 1);
                    currentContinuesUsed++;
                    return true;
                }
            }

            // Use gems
            int playerGems = PlayerPrefs.GetInt("PlayerGems", 0);
            if (playerGems >= continueCost)
            {
                PlayerPrefs.SetInt("PlayerGems", playerGems - continueCost);
                currentContinuesUsed++;
                return true;
            }

            return false;
        }

        private void NotifyGameToRestart()
        {
            // Find and notify game flow controller to restart level
            var gameFlowController = FindObjectOfType<Core.GameFlowController>();
            if (gameFlowController != null)
            {
                gameFlowController.RestartLevel();
            }
            else
            {
                // Fallback: reload scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Reset continue count for new level
        /// </summary>
        public void ResetContinues()
        {
            currentContinuesUsed = 0;
            isContinueAvailable = false;
            HideContinuePanel();
        }

        /// <summary>
        /// Called when player explicitly quits after game over
        /// </summary>
        public void OnPlayerQuit()
        {
            HideContinuePanel();
        }

        /// <summary>
        /// Get continue cost for UI display
        /// </summary>
        public int GetContinueCost()
        {
            return continueCost;
        }

        /// <summary>
        /// Set continue cost (for events/promotions)
        /// </summary>
        public void SetContinueCost(int cost)
        {
            continueCost = Mathf.Max(0, cost);
            UpdateContinueUI();
        }

        /// <summary>
        /// Add free continues (for rewards)
        /// </summary>
        public void AddFreeContinues(int count)
        {
            // This could be used for daily rewards or achievements
            // For now, just log it
            Debug.Log($"[ContinueSystem] Added {count} free continues");
        }

        /// <summary>
        /// Skip continue panel and fail directly
        /// </summary>
        public void SkipContinue()
        {
            HideContinuePanel();
            OnContinueFailed?.Invoke(currentLevelId, "Skipped");
        }
    }

    #region Continue UI Component (for Unity UI)

    /// <summary>
    /// Helper component to easily reference continue UI elements
    /// </summary>
    public class ContinueUI : MonoBehaviour
    {
        [SerializeField] private ContinueSystem continueSystem;

        private void Start()
        {
            if (continueSystem == null)
            {
                continueSystem = ContinueSystem.Instance;
            }
        }

        public void OnSkipButton()
        {
            continueSystem?.SkipContinue();
        }

        public void OnQuitButton()
        {
            continueSystem?.OnPlayerQuit();
            // Navigate to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    #endregion
}
