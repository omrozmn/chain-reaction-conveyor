using UnityEngine;
using System.Collections.Generic;
using System;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// PHASE 2.1: Tracks player performance metrics and detects difficulty patterns.
    /// </summary>
    public class DifficultyEngine : MonoBehaviour
    {
        public static DifficultyEngine Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int windowSize = 10;           // Games to track for win rate
        [SerializeField] private int spikeThreshold = 3;          // Consecutive failures to trigger spike
        [SerializeField] private int recoveryThreshold = 3;     // Consecutive wins to trigger recovery
        [SerializeField] private float difficultyStep = 0.1f;   // How much to adjust per step

        [Header("Base Difficulty - NEW PLAYERS")]
        [SerializeField] private float baseDifficulty = 0.4f;    // FIXED: 0.3-0.5 range for new players (was 1.0f)
        [SerializeField] private float minDifficulty = 0.3f;     // Minimum floor
        [SerializeField] private float maxDifficulty = 2.0f;     // Maximum ceiling

        [Header("Current State (Read-Only)")]
        [SerializeField] private float currentWinRate = 0.5f;
        [SerializeField] private int consecutiveFailures = 0;
        [SerializeField] private int consecutiveWins = 0;
        [SerializeField] private bool isSpikeDetected = false;
        [SerializeField] private bool isRecovering = false;
        [SerializeField] private float currentDifficulty = 0.4f;  // FIXED: Start at baseDifficulty

        private Queue<bool> recentResults = new Queue<bool>();
        private bool isInitialized = false;

        public event Action<float> OnDifficultyChanged;
        public event Action<bool> OnSpikeDetected;     // true = spike, false = recovered

        public float CurrentWinRate => currentWinRate;
        public bool IsSpikeDetected => isSpikeDetected;
        public bool IsRecovering => isRecovering;
        public float CurrentDifficulty => currentDifficulty;
        public float BaseDifficulty => baseDifficulty;

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
            // Initialize with base difficulty for new players
            currentDifficulty = baseDifficulty;
            isInitialized = true;
            Debug.Log($"[DifficultyEngine] Initialized with base difficulty: {baseDifficulty}");
        }

        /// <summary>
        /// Record a game result (true = win, false = loss)
        /// </summary>
        public void RecordResult(bool isWin)
        {
            recentResults.Enqueue(isWin);

            // Keep only the last 'windowSize' results
            if (recentResults.Count > windowSize)
            {
                recentResults.Dequeue();
            }

            UpdateConsecutiveCounts(isWin);
            UpdateWinRate();
            DetectPatterns();
        }

        private void UpdateConsecutiveCounts(bool isWin)
        {
            if (isWin)
            {
                consecutiveWins++;
                consecutiveFailures = 0;
            }
            else
            {
                consecutiveFailures++;
                consecutiveWins = 0;
            }
        }

        private void UpdateWinRate()
        {
            if (recentResults.Count == 0)
            {
                currentWinRate = 0.5f;
                return;
            }

            int wins = 0;
            foreach (bool result in recentResults)
            {
                if (result) wins++;
            }
            currentWinRate = (float)wins / recentResults.Count;
        }

        private void DetectPatterns()
        {
            // Check for spike (struggling)
            if (consecutiveFailures >= spikeThreshold && !isSpikeDetected)
            {
                isSpikeDetected = true;
                isRecovering = false;
                OnSpikeDetected?.Invoke(true);
                AdjustDifficulty(-difficultyStep);  // Make easier
                Debug.Log($"[DifficultyEngine] SPIKE DETECTED! {consecutiveFailures} failures. Reducing difficulty to {currentDifficulty}");
            }
            // Check for recovery (doing well)
            else if (consecutiveWins >= recoveryThreshold && (isSpikeDetected || currentDifficulty < 1.0f))
            {
                bool wasSpike = isSpikeDetected;
                isSpikeDetected = false;
                isRecovering = true;
                
                if (wasSpike)
                {
                    OnSpikeDetected?.Invoke(false);
                }
                
                AdjustDifficulty(difficultyStep);  // Make harder
                Debug.Log($"[DifficultyEngine] RECOVERY! {consecutiveWins} wins. Increasing difficulty to {currentDifficulty}");
            }
            // Normal play - gradual difficulty shift based on win rate
            else
            {
                isRecovering = false;
                
                if (currentWinRate > 0.7f && currentDifficulty < maxDifficulty)
                {
                    AdjustDifficulty(difficultyStep * 0.5f);  // Slight increase
                }
                else if (currentWinRate < 0.3f && currentDifficulty > minDifficulty)
                {
                    AdjustDifficulty(-difficultyStep * 0.5f); // Slight decrease
                }
            }
        }

        private void AdjustDifficulty(float delta)
        {
            currentDifficulty = Mathf.Clamp(currentDifficulty + delta, minDifficulty, maxDifficulty);
            OnDifficultyChanged?.Invoke(currentDifficulty);
        }

        /// <summary>
        /// Apply difficulty from LevelDef - used for monetization anchors
        /// </summary>
        public void ApplyLevelDefDifficulty(Models.LevelDef levelDef)
        {
            if (levelDef == null) return;

            // Apply monetization anchor - easier levels for monetization
            if (levelDef.monetizationAnchor)
            {
                currentDifficulty = Mathf.Min(currentDifficulty, baseDifficulty);
                Debug.Log($"[DifficultyEngine] Monetization anchor applied: difficulty set to {currentDifficulty}");
            }

            // Apply spike/recovery flags from LevelDef
            if (levelDef.isSpike)
            {
                isSpikeDetected = true;
                OnSpikeDetected?.Invoke(true);
            }
            
            if (levelDef.isRecovery)
            {
                isRecovering = true;
            }

            // Apply target win rate from LevelDef
            if (levelDef.targetWinRate > 0)
            {
                // Adjust difficulty toward target win rate
                float targetDifficulty = levelDef.targetWinRate * 2f; // 0.3 -> 0.6, 0.7 -> 1.4
                currentDifficulty = Mathf.Clamp(targetDifficulty, minDifficulty, maxDifficulty);
                OnDifficultyChanged?.Invoke(currentDifficulty);
                Debug.Log($"[DifficultyEngine] Level target win rate {levelDef.targetWinRate} applied, difficulty: {currentDifficulty}");
            }
        }

        /// <summary>
        /// Reset stats for new session
        /// </summary>
        public void ResetStats()
        {
            recentResults.Clear();
            currentWinRate = 0.5f;
            consecutiveFailures = 0;
            consecutiveWins = 0;
            isSpikeDetected = false;
            isRecovering = false;
            currentDifficulty = baseDifficulty;  // FIXED: Reset to baseDifficulty instead of 1.0f
        }

        /// <summary>
        /// Get current difficulty multiplier for use in game
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            return currentDifficulty;
        }

        /// <summary>
        /// Set base difficulty for new players (can be adjusted based on player history)
        /// </summary>
        public void SetBaseDifficulty(float baseDiff)
        {
            baseDifficulty = Mathf.Clamp(baseDiff, 0.3f, 0.5f);  // Enforce 0.3-0.5 range
            if (!isInitialized)
            {
                currentDifficulty = baseDifficulty;
            }
            Debug.Log($"[DifficultyEngine] Base difficulty set to: {baseDifficulty}");
        }
    }
}
