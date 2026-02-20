using UnityEngine;
using System.Collections.Generic;
using System;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// PHASE 2.2: Detects "near-miss" moments for dynamic feedback and difficulty tuning.
    /// </summary>
    public class NearMissEngine : MonoBehaviour
    {
        public static NearMissEngine Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private float nearMissDistance = 0.5f;    // Distance considered "near miss"
        [SerializeField] private float nearMissTimeWindow = 0.5f; // Time window to track near-misses
        [SerializeField] private int maxNearMissesPerSession = 20; // Max to track

        [Header("Current State (Read-Only)")]
        [SerializeField] private int nearMissCount = 0;
        [SerializeField] private float nearestDistance = float.MaxValue;
        [SerializeField] private bool isNearMissStreak = false;
        [SerializeField] private int nearMissStreakCount = 0;

        private Queue<NearMissEvent> recentNearMisses = new Queue<NearMissEvent>();
        private List<NearMissEvent> sessionNearMisses = new List<NearMissEvent>();

        public struct NearMissEvent
        {
            public Vector3 position;
            public float distance;
            public float time;
            public string objectName;
        }

        public event Action<NearMissEvent> OnNearMissDetected;
        public event Action<int> OnNearMissStreakChanged;

        public int NearMissCount => nearMissCount;
        public float NearestDistance => nearestDistance;
        public bool IsNearMissStreak => isNearMissStreak;
        public int NearMissStreakCount => nearMissStreakCount;

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
        /// Check if an object is a "near miss" relative to a target position
        /// </summary>
        /// <param name="objectPos">Position of the moving object</param>
        /// <param name="targetPos">Position of the target (e.g., pocket, obstacle)</param>
        /// <param name="objectName">Name for debugging</param>
        public void CheckNearMiss(Vector3 objectPos, Vector3 targetPos, string objectName = "Object")
        {
            float distance = Vector3.Distance(objectPos, targetPos);

            // Check if this qualifies as a near miss
            if (distance <= nearMissDistance && distance > 0.01f)
            {
                RecordNearMiss(objectPos, distance, objectName);
            }
        }

        /// <summary>
        /// Check multiple targets and record the closest near miss
        /// </summary>
        public void CheckNearMisses(Vector3 objectPos, Vector3[] targetPositions, string objectName = "Object")
        {
            float closestDistance = float.MaxValue;
            Vector3 closestPosition = Vector3.zero;

            foreach (var targetPos in targetPositions)
            {
                float distance = Vector3.Distance(objectPos, targetPos);
                if (distance < closestDistance && distance <= nearMissDistance && distance > 0.01f)
                {
                    closestDistance = distance;
                    closestPosition = targetPos;
                }
            }

            if (closestDistance <= nearMissDistance)
            {
                RecordNearMiss(closestPosition, closestDistance, objectName);
            }
        }

        private void RecordNearMiss(Vector3 position, float distance, string objectName)
        {
            NearMissEvent newEvent = new NearMissEvent
            {
                position = position,
                distance = distance,
                time = Time.time,
                objectName = objectName
            };

            // Update tracking
            sessionNearMisses.Add(newEvent);
            recentNearMisses.Enqueue(newEvent);

            // Keep only recent events within time window
            while (recentNearMisses.Count > 0 && Time.time - recentNearMisses.Peek().time > nearMissTimeWindow)
            {
                recentNearMisses.Dequeue();
            }

            // Limit total tracked
            if (sessionNearMisses.Count > maxNearMissesPerSession)
            {
                sessionNearMisses.RemoveAt(0);
            }

            // Update stats
            nearMissCount++;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
            }

            // Check for streak (multiple near-misses in quick succession)
            CheckStreak();

            // Fire event
            OnNearMissDetected?.Invoke(newEvent);
            
            Debug.Log($"[NearMissEngine] NEAR MISS! {objectName} at distance {distance:F3}");
        }

        private void CheckStreak()
        {
            // Count near-misses in recent window
            int recentCount = 0;
            float windowStart = Time.time - nearMissTimeWindow;

            foreach (var evt in sessionNearMisses)
            {
                if (evt.time >= windowStart)
                {
                    recentCount++;
                }
            }

            if (recentCount >= 3 && !isNearMissStreak)
            {
                isNearMissStreak = true;
                OnNearMissStreakChanged?.Invoke(recentCount);
            }
            else if (recentCount < 2 && isNearMissStreak)
            {
                isNearMissStreak = false;
                OnNearMissStreakChanged?.Invoke(recentCount);
            }

            nearMissStreakCount = recentCount;
        }

        /// <summary>
        /// Get all near-miss events from current session
        /// </summary>
        public List<NearMissEvent> GetSessionNearMisses()
        {
            return new List<NearMissEvent>(sessionNearMisses);
        }

        /// <summary>
        /// Get near-miss rate (per minute) for difficulty calibration
        /// </summary>
        public float GetNearMissRatePerMinute()
        {
            if (sessionNearMisses.Count < 2) return 0f;

            float sessionDuration = sessionNearMisses[sessionNearMisses.Count - 1].time - sessionNearMisses[0].time;
            if (sessionDuration <= 0) return 0f;

            return (sessionNearMisses.Count / sessionDuration) * 60f;
        }

        /// <summary>
        /// Reset for new session
        /// </summary>
        public void ResetSession()
        {
            recentNearMisses.Clear();
            sessionNearMisses.Clear();
            nearMissCount = 0;
            nearestDistance = float.MaxValue;
            isNearMissStreak = false;
            nearMissStreakCount = 0;
        }

        /// <summary>
        /// Configure thresholds at runtime
        /// </summary>
        public void SetThresholds(float distance, float timeWindow)
        {
            nearMissDistance = Mathf.Max(0.1f, distance);
            nearMissTimeWindow = Mathf.Max(0.1f, timeWindow);
        }
    }
}
