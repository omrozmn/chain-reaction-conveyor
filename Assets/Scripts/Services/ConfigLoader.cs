using UnityEngine;
using System;
using System.IO;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Services
{
    /// <summary>
    /// Loads level definitions from JSON or ScriptableObject
    /// </summary>
    public class ConfigLoader
    {
        public static ConfigLoader Instance { get; private set; } = new ConfigLoader();

        private ConfigLoader() { }

        public LevelDef LoadLevel(int levelId)
        {
            // Try to load from JSON first
            string jsonPath = GetLevelJsonPath(levelId);
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                var level = JsonUtility.FromJson<LevelDef>(json);
                UnityEngine.Debug.Log($"[ConfigLoader] Loaded level {levelId} from JSON");
                return level;
            }

            // Fallback to default
            UnityEngine.Debug.Log($"[ConfigLoader] Using default level {levelId}");
            return CreateDefaultLevel(levelId);
        }

        public void SaveLevel(LevelDef level)
        {
            string jsonPath = GetLevelJsonPath(level.levelId);
            string directory = Path.GetDirectoryName(jsonPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(level, true);
            File.WriteAllText(jsonPath, json);
            UnityEngine.Debug.Log($"[ConfigLoader] Saved level {level.levelId} to JSON");
        }

        public LevelDef[] LoadAllLevels()
        {
            string levelsPath = GetLevelsDirectory();
            if (!Directory.Exists(levelsPath))
            {
                return Array.Empty<LevelDef>();
            }

            var levels = new System.Collections.Generic.List<LevelDef>();
            foreach (var file in Directory.GetFiles(levelsPath, "*.json"))
            {
                string json = File.ReadAllText(file);
                var level = JsonUtility.FromJson<LevelDef>(json);
                levels.Add(level);
            }

            return levels.ToArray();
        }

        private string GetLevelJsonPath(int levelId)
        {
            return Path.Combine(GetLevelsDirectory(), $"level_{levelId:D3}.json");
        }

        private string GetLevelsDirectory()
        {
            return Path.Combine(Application.dataPath, "Config", "Levels");
        }

        private LevelDef CreateDefaultLevel(int levelId)
        {
            return new LevelDef
            {
                levelId = levelId,
                seed = Guid.NewGuid().GetHashCode(),
                boardWidth = 6,
                boardHeight = 8,
                minCluster = 3,
                spawnInterval = 1.5f,
                conveyorSpeed = 1.0f,
                targetProgress = 20,
                targetType = TargetType.FillSlots,
                isSpike = false,
                isRecovery = false,
                isAnchor = false,
                maxSpawn = 50,
                pocketCount = 5,
                pocketCapacity = 3,
                targetWinRate = 0.3f,
                targetFailRate = 0.7f,
                monetizationAnchor = false
            };
        }

        public GlobalConfig LoadGlobalConfig()
        {
            string configPath = Path.Combine(Application.dataPath, "Config", "global_config.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                return JsonUtility.FromJson<GlobalConfig>(json);
            }
            return CreateDefaultGlobalConfig();
        }

        private GlobalConfig CreateDefaultGlobalConfig()
        {
            return new GlobalConfig
            {
                baseMinCluster = 3,
                baseSpawnInterval = 1.5f,
                baseConveyorSpeed = 1.0f,
                spikeMultiplier = 1.2f,
                recoveryMultiplier = 0.8f,
                adaptiveFailThreshold = 3,
                adaptiveSpawnBonus = 0.15f,
                adaptiveSpeedPenalty = 0.1f,
                adaptiveBonusBonus = 0.1f,
                nearMissThreshold = 0.8f,
                nearMissSpawnWeightBonus = 0.2f
            };
        }
    }

    [Serializable]
    public class GlobalConfig
    {
        public float baseMinCluster = 3;
        public float baseSpawnInterval = 1.5f;
        public float baseConveyorSpeed = 1.0f;
        
        public float spikeMultiplier = 1.2f;
        public float recoveryMultiplier = 0.8f;
        
        public int adaptiveFailThreshold = 3;
        public float adaptiveSpawnBonus = 0.15f;
        public float adaptiveSpeedPenalty = 0.1f;
        public float adaptiveBonusBonus = 0.1f;
        
        public float nearMissThreshold = 0.8f;
        public float nearMissSpawnWeightBonus = 0.2f;
    }
}
