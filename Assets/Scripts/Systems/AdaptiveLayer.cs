using UnityEngine;
using System.Collections.Generic;
using System;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// PHASE 2.3: Dynamic difficulty adjustment layer that coordinates 
    /// DifficultyEngine and NearMissEngine for seamless gameplay experience.
    /// </summary>
    public class AdaptiveLayer : MonoBehaviour
    {
        public static AdaptiveLayer Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private DifficultyEngine difficultyEngine;
        [SerializeField] private NearMissEngine nearMissEngine;

        [Header("Configuration")]
        [SerializeField] private bool enableAdaptiveDifficulty = true;
        [SerializeField] private float adaptationUpdateInterval = 1.0f; // Seconds between updates
        [SerializeField] private float nearMissPenaltyThreshold = 2.5f;    // FIXED: Was 5f, now 2-3 for better sensitivity
        
        [Header("Difficulty Parameters")]
        [SerializeField] private float conveyorSpeedMultiplier = 1.0f;
        [SerializeField] private float spawnRateMultiplier = 1.0f;
        [SerializeField] private float obstacleDensityMultiplier = 1.0f;

        private float updateTimer = 0f;
        private bool isInitialized = false;

        // Current adaptation state
        public float ConveyorSpeedMultiplier => conveyorSpeedMultiplier;
        public float SpawnRateMultiplier => spawnRateMultiplier;
        public float ObstacleDensityMultiplier => obstacleDensityMultiplier;
        public bool IsAdaptiveEnabled => enableAdaptiveDifficulty;

        public event Action<float> OnAdaptationChanged;

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
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            // Get or create DifficultyEngine
            if (difficultyEngine == null)
            {
                difficultyEngine = GetComponent<DifficultyEngine>();
                if (difficultyEngine == null)
                {
                    difficultyEngine = gameObject.AddComponent<DifficultyEngine>();
                }
            }

            // Get or create NearMissEngine
            if (nearMissEngine == null)
            {
                nearMissEngine = GetComponent<NearMissEngine>();
                if (nearMissEngine == null)
                {
                    nearMissEngine = gameObject.AddComponent<NearMissEngine>();
                }
            }

            // Subscribe to events
            if (difficultyEngine != null)
            {
                difficultyEngine.OnDifficultyChanged += HandleDifficultyChanged;
                difficultyEngine.OnSpikeDetected += HandleSpikeDetected;
            }

            if (nearMissEngine != null)
            {
                nearMissEngine.OnNearMissDetected += HandleNearMissDetected;
                nearMissEngine.OnNearMissStreakChanged += HandleNearMissStreak;
            }
            
            isInitialized = true;
            Debug.Log("[AdaptiveLayer] Systems initialized");
        }

        private void Update()
        {
            if (!enableAdaptiveDifficulty || !isInitialized) return;

            updateTimer += Time.deltaTime;
            if (updateTimer >= adaptationUpdateInterval)
            {
                updateTimer = 0f;
                PerformAdaptation();
            }
        }

        /// <summary>
        /// Main adaptation logic - called periodically
        /// </summary>
        private void PerformAdaptation()
        {
            if (difficultyEngine == null || nearMissEngine == null) return;

            // Get current metrics
            float difficulty = difficultyEngine.CurrentDifficulty;
            float nearMissRate = nearMissEngine.GetNearMissRatePerMinute();

            // Calculate target multipliers based on difficulty (1.0 = baseline)
            float targetSpeedMult = GetTargetSpeedMultiplier(difficulty);
            float targetSpawnMult = GetTargetSpawnMultiplier(difficulty);
            float targetObstacleMult = GetTargetObstacleMultiplier(difficulty);

            // Adjust for near-miss patterns
            if (nearMissRate > nearMissPenaltyThreshold)
            {
                // Player is having close calls - slightly reduce difficulty
                targetSpeedMult *= 0.95f;
                targetSpawnMult *= 0.9f;
                Debug.Log($"[AdaptiveLayer] Near-miss rate high ({nearMissRate:F1}/min) - reducing intensity");
            }

            // Smooth interpolation to new values
            conveyorSpeedMultiplier = Mathf.Lerp(conveyorSpeedMultiplier, targetSpeedMult, 0.1f);
            spawnRateMultiplier = Mathf.Lerp(spawnRateMultiplier, targetSpawnMult, 0.1f);
            obstacleDensityMultiplier = Mathf.Lerp(obstacleDensityMultiplier, targetObstacleMult, 0.1f);

            OnAdaptationChanged?.Invoke(difficulty);
        }

        private float GetTargetSpeedMultiplier(float difficulty)
        {
            // Higher difficulty = faster conveyor
            return Mathf.Clamp(0.5f + (difficulty * 0.5f), 0.5f, 1.5f);
        }

        private float GetTargetSpawnMultiplier(float difficulty)
        {
            // Higher difficulty = more frequent spawns
            return Mathf.Clamp(0.7f + (difficulty * 0.4f), 0.6f, 1.4f);
        }

        private float GetTargetObstacleMultiplier(float difficulty)
        {
            // Higher difficulty = more obstacles
            return Mathf.Clamp(0.5f + (difficulty * 0.5f), 0.4f, 1.6f);
        }

        // Event handlers
        private void HandleDifficultyChanged(float newDifficulty)
        {
            Debug.Log($"[AdaptiveLayer] Difficulty changed to {newDifficulty:F2}");
        }

        private void HandleSpikeDetected(bool isSpike)
        {
            if (isSpike)
            {
                // Player struggling - reduce game intensity immediately
                conveyorSpeedMultiplier *= 0.9f;
                spawnRateMultiplier *= 0.85f;
                Debug.Log("[AdaptiveLayer] SPIKE detected - reducing game intensity");
            }
            else
            {
                // Player recovering - can start increasing again
                Debug.Log("[AdaptiveLayer] Player recovered - normal adaptation resumed");
            }
        }

        private void HandleNearMissDetected(NearMissEngine.NearMissEvent nearMiss)
        {
            // Could trigger visual/audio feedback here
            // Debug.Log($"[AdaptiveLayer] Near miss feedback: {nearMiss.objectName} at {nearMiss.distance:F3}");
        }

        private void HandleNearMissStreak(int streakCount)
        {
            if (streakCount >= 3)
            {
                Debug.Log($"[AdaptiveLayer] Near-miss streak: {streakCount}");
            }
        }

        /// <summary>
        /// Called when player wins - record for difficulty tracking
        /// </summary>
        public void RecordWin()
        {
            if (difficultyEngine != null)
            {
                difficultyEngine.RecordResult(true);
            }
        }

        /// <summary>
        /// Called when player loses - record for difficulty tracking
        /// </summary>
        public void RecordLoss()
        {
            if (difficultyEngine != null)
            {
                difficultyEngine.RecordResult(false);
            }
        }

        /// <summary>
        /// Enable/disable adaptive difficulty at runtime
        /// </summary>
        public void SetAdaptiveEnabled(bool enabled)
        {
            enableAdaptiveDifficulty = enabled;
            
            if (!enabled)
            {
                // Reset to baseline
                conveyorSpeedMultiplier = 1.0f;
                spawnRateMultiplier = 1.0f;
                obstacleDensityMultiplier = 1.0f;
            }
            
            Debug.Log($"[AdaptiveLayer] Adaptive difficulty {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Get combined difficulty factor for game systems
        /// </summary>
        public float GetCombinedDifficultyFactor()
        {
            return (conveyorSpeedMultiplier + spawnRateMultiplier + obstacleDensityMultiplier) / 3f;
        }

        private void OnDestroy()
        {
            if (difficultyEngine != null)
            {
                difficultyEngine.OnDifficultyChanged -= HandleDifficultyChanged;
                difficultyEngine.OnSpikeDetected -= HandleSpikeDetected;
            }

            if (nearMissEngine != null)
            {
                nearMissEngine.OnNearMissDetected -= HandleNearMissDetected;
                nearMissEngine.OnNearMissStreakChanged -= HandleNearMissStreak;
            }
        }
    }
}
