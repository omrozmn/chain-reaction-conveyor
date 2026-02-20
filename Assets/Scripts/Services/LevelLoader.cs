using UnityEngine;
using System;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Services
{
    /// <summary>
    /// Manages level loading and initialization
    /// </summary>
    public class LevelLoader
    {
        public static LevelLoader Instance { get; private set; } = new LevelLoader();

        private LevelDef _currentLevel;
        private int _currentLevelId;

        public event Action<LevelDef> OnLevelLoaded;
        public event Action OnLevelUnloaded;

        private LevelLoader() { }

        public void LoadLevel(int levelId)
        {
            _currentLevelId = levelId;
            
            // Load level definition from ConfigLoader
            _currentLevel = ConfigLoader.Instance.LoadLevel(levelId);
            
            // Apply difficulty modifiers based on level flags
            ApplyDifficultyModifiers();

            // Initialize RNG with level seed
            RNG.Initialize(_currentLevel.seed);

            Debug.Log($"[LevelLoader] Level {levelId} loaded (seed: {_currentLevel.seed})");
            OnLevelLoaded?.Invoke(_currentLevel);
        }

        public void UnloadLevel()
        {
            _currentLevel = null;
            OnLevelUnloaded?.Invoke();
        }

        public LevelDef GetCurrentLevel() => _currentLevel;
        public int GetCurrentLevelId() => _currentLevelId;

        private void ApplyDifficultyModifiers()
        {
            if (_currentLevel == null) return;

            // Get global config
            var globalConfig = ConfigLoader.Instance.LoadGlobalConfig();

            // Apply spike modifiers
            if (_currentLevel.isSpike)
            {
                _currentLevel.spawnInterval *= globalConfig.spikeMultiplier;
                _currentLevel.conveyorSpeed *= globalConfig.spikeMultiplier;
                Debug.Log("[LevelLoader] Spike modifiers applied");
            }

            // Apply recovery modifiers
            if (_currentLevel.isRecovery)
            {
                _currentLevel.spawnInterval *= globalConfig.recoveryMultiplier;
                _currentLevel.conveyorSpeed *= globalConfig.recoveryMultiplier;
                Debug.Log("[LevelLoader] Recovery modifiers applied");
            }
        }

        public void ApplyAdaptiveModifiers(int failCount)
        {
            if (_currentLevel == null) return;

            var globalConfig = ConfigLoader.Instance.LoadGlobalConfig();

            if (failCount >= globalConfig.adaptiveFailThreshold)
            {
                float spawnBonus = 1f + (failCount - globalConfig.adaptiveFailThreshold + 1) * globalConfig.adaptiveSpawnBonus;
                float speedPenalty = 1f - (failCount - globalConfig.adaptiveFailThreshold + 1) * globalConfig.adaptiveSpeedPenalty;

                _currentLevel.spawnInterval *= spawnBonus;
                _currentLevel.conveyorSpeed *= speedPenalty;

                Debug.Log($"[LevelLoader] Adaptive modifiers applied (failCount: {failCount})");
            }
        }

        public void GenerateLevel(int levelId, int difficulty = 0)
        {
            // Procedural level generation for 300+ levels
            _currentLevel = new LevelDef
            {
                levelId = levelId,
                seed = Guid.NewGuid().GetHashCode(),
                boardWidth = 6 + (difficulty / 20), // Max 8
                boardHeight = 8 + (difficulty / 15), // Max 12
                minCluster = 3,
                spawnInterval = Mathf.Max(0.8f, 1.5f - (difficulty * 0.02f)),
                conveyorSpeed = Mathf.Min(2f, 1f + (difficulty * 0.01f)),
                targetProgress = 15 + difficulty,
                targetType = GetTargetTypeForDifficulty(difficulty),
                isSpike = (difficulty % 10 == 9),
                isRecovery = (difficulty % 10 == 0 && difficulty > 0),
                isAnchor = (difficulty % 30 == 0),
                maxSpawn = 40 + difficulty * 2,
                pocketCount = 5,
                pocketCapacity = 3 + (difficulty / 20),
                targetWinRate = 0.3f - (difficulty * 0.002f),
                targetFailRate = 0.7f + (difficulty * 0.002f),
                monetizationAnchor = (difficulty % 30 == 0)
            };

            _currentLevelId = levelId;
            RNG.Initialize(_currentLevel.seed);

            Debug.Log($"[LevelLoader] Generated level {levelId} (difficulty: {difficulty})");
            OnLevelLoaded?.Invoke(_currentLevel);
        }

        private TargetType GetTargetTypeForDifficulty(int difficulty)
        {
            if (difficulty < 10) return TargetType.FillSlots;
            if (difficulty < 25) return TargetType.ClearLocked;
            if (difficulty < 40) return TargetType.DeliverToGate;
            return TargetType.Hybrid;
        }
    }
}
