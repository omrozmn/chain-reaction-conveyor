using UnityEngine;
using System.Collections.Generic;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Handles level loading from JSON and ScriptableObject sources
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private TextAsset jsonLevelsFile;
        [SerializeField] private int defaultLevelId = 1;
        
        private List<LevelDef> _levelCache;
        private Dictionary<int, LevelDef> _levelDict;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _levelDict = new Dictionary<int, LevelDef>();
        }

        private void Start()
        {
            LoadAllLevels();
        }

        /// <summary>
        /// Load all levels from JSON file
        /// </summary>
        public void LoadAllLevels()
        {
            _levelCache = new List<LevelDef>();
            _levelDict.Clear();

            if (jsonLevelsFile != null)
            {
                try
                {
                    var levels = JsonUtility.FromJson<LevelDefCollection>("{\"levels\":" + jsonLevelsFile.text + "}");
                    if (levels != null && levels.levels != null)
                    {
                        foreach (var level in levels.levels)
                        {
                            AddLevel(level);
                        }
                        Debug.Log($"[LevelLoader] Loaded {_levelDict.Count} levels from JSON");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LevelLoader] Failed to parse JSON: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[LevelLoader] No JSON file assigned, using default levels");
                // Add some default levels for testing
                AddDefaultLevels();
            }
        }

        private void AddDefaultLevels()
        {
            for (int i = 1; i <= 10; i++)
            {
                AddLevel(CreateDefaultLevel(i));
            }
        }

        private void AddLevel(LevelDef level)
        {
            if (level == null) return;
            
            _levelDict[level.levelId] = level;
            _levelCache.Add(level);
        }

        /// <summary>
        /// Load a specific level by ID
        /// </summary>
        public LevelDef LoadLevel(int levelId)
        {
            if (_levelDict.TryGetValue(levelId, out var level))
            {
                return level;
            }

            Debug.LogWarning($"[LevelLoader] Level {levelId} not found, returning default");
            return CreateDefaultLevel(levelId);
        }

        /// <summary>
        /// Get total number of levels
        /// </summary>
        public int GetLevelCount() => _levelDict.Count;

        /// <summary>
        /// Check if level exists
        /// </summary>
        public bool HasLevel(int levelId) => _levelDict.ContainsKey(levelId);

        /// <summary>
        /// Get next level ID, wraps around to 1
        /// </summary>
        public int GetNextLevelId(int currentId)
        {
            int nextId = currentId + 1;
            if (nextId > GetLevelCount())
            {
                nextId = 1; // Loop back to start
            }
            return nextId;
        }

        private LevelDef CreateDefaultLevel(int levelId)
        {
            return new LevelDef
            {
                levelId = levelId,
                seed = levelId * 1000,
                boardWidth = 6,
                boardHeight = 8,
                minCluster = 3,
                spawnInterval = 1.5f,
                conveyorSpeed = 1.0f,
                targetProgress = 20 + (levelId * 2),
                isSpike = IsSpikeLevel(levelId),
                isRecovery = IsRecoveryLevel(levelId)
            };
        }

        private bool IsSpikeLevel(int levelId)
        {
            // Every 10th level is a spike
            return levelId % 10 == 0;
        }

        private bool IsRecoveryLevel(int levelId)
        {
            // Levels after spike are recovery
            return levelId > 1 && (levelId - 1) % 10 == 0;
        }

        /// <summary>
        /// Reload levels from file (for development)
        /// </summary>
        public void ReloadLevels()
        {
            LoadAllLevels();
        }

        /// <summary>
        /// Get all levels as list
        /// </summary>
        public List<LevelDef> GetAllLevels() => new List<LevelDef>(_levelCache);
    }

    /// <summary>
    /// Helper class for JSON deserialization
    /// </summary>
    [System.Serializable]
    public class LevelDefCollection
    {
        public LevelDef[] levels;
    }
}
