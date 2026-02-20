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
        [SerializeField] private int spikeThreshold = 3;       // Consecutive failures to trigger spike
        [SerializeField] private int recoveryThreshold = 3;    // Consecutive wins to trigger recovery
        [SerializeField] private float difficultyStep = 0.1f;  // How much to adjust per step

        [Header("Current State (Read-Only)")]
        [SerializeField] private float currentWinRate = 0.5f;
        [SerializeField] private int consecutiveFailures = 0;
        [SerializeField] private int consecutiveWins = 0;
        [SerializeField] private bool isSpikeDetected = false;
        [SerializeField] private bool isRecovering = false;
        [SerializeField] private float currentDifficulty = 1.0f;

        private Queue<bool> recentResults = new Queue<bool>();

        public event Action<float> OnDifficultyChanged;
        public event Action<bool> OnSpikeDetected;     // true = spike, false = recovered

        public float CurrentWinRate => currentWinRate;
        public bool IsSpikeDetected => isSpikeDetected;
        public bool IsRecovering => isRecovering;
        public float CurrentDifficulty => currentDifficulty;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
                
                if (currentWinRate > 0.7f && currentDifficulty < 2.0f)
                {
                    AdjustDifficulty(difficultyStep * 0.5f);  // Slight increase
                }
                else if (currentWinRate < 0.3f && currentDifficulty > 0.3f)
                {
                    AdjustDifficulty(-difficultyStep * 0.5f); // Slight decrease
                }
            }
        }

        private void AdjustDifficulty(float delta)
        {
            currentDifficulty = Mathf.Clamp(currentDifficulty + delta, 0.3f, 2.0f);
            OnDifficultyChanged?.Invoke(currentDifficulty);
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
            currentDifficulty = 1.0f;
        }

        /// <summary>
        /// Get current difficulty multiplier for use in game
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            return currentDifficulty;
        }
    }
}
