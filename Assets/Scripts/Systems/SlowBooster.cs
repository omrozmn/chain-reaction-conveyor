using UnityEngine;
using System;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Slow Booster - Temporarily slows down the conveyor belt
    /// </summary>
    [RequireComponent(typeof(BoosterManager))]
    public class SlowBooster : MonoBehaviour
    {
        [Header("Slow Settings")]
        [SerializeField] private float slowDuration = 5f;
        [SerializeField] private float slowFactor = 0.3f; // 30% speed
        [SerializeField] private bool canStack = false; // Can extend duration if already active

        // Reference to BoosterManager
        private BoosterManager boosterManager;

        // State
        private bool isSlowActive = false;
        private float slowTimer = 0f;
        private float originalTimeScale = 1f;

        // Events
        public event Action<float> OnSlowStarted;
        public event Action<float> OnSlowEnded;
        public event Action<float> OnSlowExtended;

        // Property for UI
        public bool IsActive => isSlowActive;
        public float RemainingTime => isSlowActive ? slowTimer : 0f;
        public float Duration => slowDuration;

        private void Awake()
        {
            boosterManager = GetComponent<BoosterManager>();
        }

        private void Update()
        {
            if (isSlowActive)
            {
                slowTimer -= Time.unscaledDeltaTime; // Use unscaled to work with timeScale changes
                
                if (slowTimer <= 0f)
                {
                    EndSlowEffect();
                }
            }
        }

        /// <summary>
        /// Activate slow effect
        /// </summary>
        public bool Activate()
        {
            if (!boosterManager.HasBooster(BoosterType.Slow))
            {
                Debug.LogWarning("[SlowBooster] No slow charges available");
                return false;
            }

            // Deduct charge
            boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);

            if (isSlowActive && canStack)
            {
                // Extend duration
                slowTimer = slowDuration;
                OnSlowExtended?.Invoke(slowTimer);
                Debug.Log($"[SlowBooster] Slow effect extended, new duration: {slowDuration}s");
            }
            else
            {
                // Start new slow effect
                StartSlowEffect();
            }

            return true;
        }

        private void StartSlowEffect()
        {
            // Store original time scale (might be already modified)
            originalTimeScale = Time.timeScale;
            
            // Apply slow
            Time.timeScale = slowFactor;
            isSlowActive = true;
            slowTimer = slowDuration;
            
            // Notify other systems that might be affected
            NotifySpeedChange(slowFactor);
            
            OnSlowStarted?.Invoke(slowDuration);
            Debug.Log($"[SlowBooster] Slow effect started for {slowDuration}s (speed: {slowFactor * 100}%)");
        }

        private void EndSlowEffect()
        {
            isSlowActive = false;
            slowTimer = 0f;
            
            // Restore time scale
            Time.timeScale = originalTimeScale;
            
            // Notify other systems
            NotifySpeedChange(1f);
            
            OnSlowEnded?.Invoke(0f);
            Debug.Log("[SlowBooster] Slow effect ended, speed restored to 100%");
        }

        private void NotifySpeedChange(float factor)
        {
            // Notify conveyor to adjust speed
            var conveyor = FindObjectOfType<Mechanics.ConveyorMechanic>();
            if (conveyor != null)
            {
                conveyor.SetSpeedMultiplier(factor);
            }
            
            // You can add more systems here that need to respond to slow
            // e.g., particle systems, animations, etc.
        }

        /// <summary>
        /// Force end slow effect (for level complete, game over, etc.)
        /// </summary>
        public void ForceEnd()
        {
            if (isSlowActive)
            {
                EndSlowEffect();
            }
        }

        /// <summary>
        /// Get current slow factor
        /// </summary>
        public float GetSlowFactor() => slowFactor;

        /// <summary>
        /// Set slow duration (for upgrades)
        /// </summary>
        public void SetSlowDuration(float duration)
        {
            slowDuration = Mathf.Max(1f, duration);
        }

        /// <summary>
        /// Set slow factor (for upgrades)
        /// </summary>
        public void SetSlowFactor(float factor)
        {
            slowFactor = Mathf.Clamp(factor, 0.1f, 0.9f);
            
            // If slow is active, apply new factor immediately
            if (isSlowActive)
            {
                Time.timeScale = slowFactor;
                NotifySpeedChange(slowFactor);
            }
        }

        /// <summary>
        /// Get progress of slow effect (0-1)
        /// </summary>
        public float GetProgress()
        {
            if (!isSlowActive || slowDuration <= 0f)
                return 0f;
            
            return 1f - (slowTimer / slowDuration);
        }

        /// <summary>
        /// Preview slow effect (for UI)
        /// </summary>
        public void Preview()
        {
            if (boosterManager.HasBooster(BoosterType.Slow))
            {
                Debug.Log($"[SlowBooster] Preview: Will slow conveyor to {slowFactor * 100}% for {slowDuration}s");
            }
            else
            {
                Debug.Log("[SlowBooster] Preview: No charges available");
            }
        }

        private void OnDestroy()
        {
            // Ensure time scale is restored when object is destroyed
            if (isSlowActive)
            {
                Time.timeScale = originalTimeScale;
            }
        }
    }
}
