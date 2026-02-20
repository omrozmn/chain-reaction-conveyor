using UnityEngine;
using System.Collections.Generic;
using System;
using ChainReactionConveyor.Models;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Procedural level generator with seed-based determinism
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; private set; }

        [Header("Generation Settings")]
        [SerializeField] private int defaultBoardWidth = 6;
        [SerializeField] private int defaultBoardHeight = 8;
        [SerializeField] private int defaultPocketCount = 5;
        [SerializeField] private int defaultPocketCapacity = 3;

        [Header("Difficulty Scaling")]
        [SerializeField] private float baseDifficulty = 0.4f;
        [SerializeField] private float difficultyPerLevel = 0.05f;
        [SerializeField] private float maxDifficulty = 2.0f;

        [Header("Pattern Constraints")]
        [SerializeField] private int maxSameColorInRow = 4;
        [SerializeField] private int maxClusterSize = 6;
        [SerializeField] private float clusterProbability = 0.3f;

        // Color settings
        [Header("Color Settings")]
        [SerializeField] private int colorCount = 5;
        [SerializeField] private bool allowColorRestriction = true;

        private DeterministicRandom deterministicRandom;

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
            deterministicRandom = new DeterministicRandom(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Generate a level with specified difficulty and level number
        /// </summary>
        public LevelDef GenerateLevel(int difficulty, int levelNumber)
        {
            return GenerateLevel(difficulty, levelNumber, -1); // Auto-generate seed
        }

        /// <summary>
        /// Generate a level with specified difficulty, level number, and seed
        /// </summary>
        public LevelDef GenerateLevel(int difficulty, int levelNumber, int seed)
        {
            if (seed < 0)
            {
                seed = levelNumber * 1000 + DateTime.Now.Millisecond;
            }

            deterministicRandom.SetSeed(seed);

            LevelDef level = new LevelDef
            {
                levelId = levelNumber,
                seed = seed,
                boardWidth = defaultBoardWidth,
                boardHeight = defaultBoardHeight,
                pocketCount = defaultPocketCount,
                pocketCapacity = defaultPocketCapacity,
                maxSpawn = CalculateMaxSpawn(difficulty),
                minCluster = 3,
                spawnInterval = CalculateSpawnInterval(difficulty),
                conveyorSpeed = CalculateConveyorSpeed(difficulty),
                targetProgress = CalculateTargetProgress(difficulty, levelNumber),
                targetType = TargetType.FillSlots
            };

            // Apply difficulty modifiers
            ApplyDifficultyModifiers(level, difficulty, levelNumber);

            // Ensure level is valid
            if (!ValidateGenerationConstraints(level))
            {
                Debug.LogWarning($"[LevelGenerator] Level {levelNumber} failed constraints, adjusting...");
                AdjustToMeetConstraints(level);
            }

            Debug.Log($"[LevelGenerator] Generated level {levelNumber} with seed {seed}, difficulty {difficulty}");
            return level;
        }

        /// <summary>
        /// Generate a random level based on current player performance
        /// </summary>
        public LevelDef GenerateAdaptiveLevel(int levelNumber)
        {
            float currentDifficulty = baseDifficulty;
            
            var difficultyEngine = DifficultyEngine.Instance;
            if (difficultyEngine != null)
            {
                currentDifficulty = difficultyEngine.GetDifficultyMultiplier();
            }

            return GenerateLevel(Mathf.RoundToInt(currentDifficulty * 10), levelNumber);
        }

        private int CalculateMaxSpawn(int difficulty)
        {
            // Higher difficulty = more spawns needed
            return Mathf.RoundToInt(30 + difficulty * 5);
        }

        private float CalculateSpawnInterval(int difficulty)
        {
            // Higher difficulty = faster spawns
            float baseInterval = 1.5f;
            float minInterval = 0.5f;
            return Mathf.Max(minInterval, baseInterval - difficulty * 0.1f);
        }

        private float CalculateConveyorSpeed(int difficulty)
        {
            // Higher difficulty = faster conveyor
            float baseSpeed = 1.0f;
            float maxSpeed = 2.5f;
            return Mathf.Min(maxSpeed, baseSpeed + difficulty * 0.15f);
        }

        private int CalculateTargetProgress(int difficulty, int levelNumber)
        {
            // Base target increases with level number
            int baseTarget = 15;
            int levelBonus = Mathf.RoundToInt(levelNumber * 0.5f);
            
            // Difficulty increases target
            int difficultyBonus = Mathf.RoundToInt(difficulty * 2);
            
            return baseTarget + levelBonus + difficultyBonus;
        }

        private void ApplyDifficultyModifiers(LevelDef level, int difficulty, int levelNumber)
        {
            // Apply monetization anchor for easier levels
            if (difficulty <= 3 && levelNumber % 5 == 0) // Every 5th level in easy range
            {
                level.monetizationAnchor = true;
                level.targetWinRate = 0.45f;
                level.targetProgress = Mathf.RoundToInt(level.targetProgress * 0.8f);
            }

            // Apply spike/recovery flags
            if (difficultyEngine != null)
            {
                if (difficultyEngine.IsSpikeDetected)
                {
                    level.isSpike = true;
                    level.targetWinRate = 0.5f;
                }
                else if (difficultyEngine.IsRecovering)
                {
                    level.isRecovery = true;
                }
            }

            // Scale difficulty
            level.difficultyOffset = (difficulty * difficultyPerLevel) - baseDifficulty;
        }

        private DifficultyEngine difficultyEngine
        {
            get
            {
                if (_difficultyEngine == null)
                {
                    _difficultyEngine = DifficultyEngine.Instance;
                }
                return _difficultyEngine;
            }
        }
        private DifficultyEngine _difficultyEngine;

        /// <summary>
        /// Validate that generated level meets constraints
        /// </summary>
        private bool ValidateGenerationConstraints(LevelDef level)
        {
            // Check spawn interval
            if (level.spawnInterval < 0.3f || level.spawnInterval > 3.0f)
                return false;

            // Check conveyor speed
            if (level.conveyorSpeed < 0.5f || level.conveyorSpeed > 5.0f)
                return false;

            // Check target progress
            if (level.targetProgress < 5 || level.targetProgress > 100)
                return false;

            // Check board size
            if (level.boardWidth < 4 || level.boardWidth > 10)
                return false;
            if (level.boardHeight < 5 || level.boardHeight > 12)
                return false;

            return true;
        }

        /// <summary>
        /// Adjust level to meet constraints if validation failed
        /// </summary>
        private void AdjustToMeetConstraints(LevelDef level)
        {
            level.spawnInterval = Mathf.Clamp(level.spawnInterval, 0.3f, 3.0f);
            level.conveyorSpeed = Mathf.Clamp(level.conveyorSpeed, 0.5f, 5.0f);
            level.targetProgress = Mathf.Clamp(level.targetProgress, 5, 100);
            level.boardWidth = Mathf.Clamp(level.boardWidth, 4, 10);
            level.boardHeight = Mathf.Clamp(level.boardHeight, 5, 12);
        }

        /// <summary>
        /// Generate initial board state (for testing/debugging)
        /// </summary>
        public int[,] GenerateInitialBoard(int width, int height, int seed)
        {
            deterministicRandom.SetSeed(seed);
            int[,] board = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Use patterns based on probability
                    if (deterministicRandom.Range(0f, 1f) < clusterProbability)
                    {
                        // Create a small cluster
                        board[x, y] = GenerateClusterColor(board, x, y, width, height);
                    }
                    else
                    {
                        // Random color
                        board[x, y] = deterministicRandom.Range(0, colorCount);
                    }
                }
            }

            return board;
        }

        private int GenerateClusterColor(int[,] board, int x, int y, int width, int height)
        {
            // Check neighbors and match one of them with higher probability
            List<int> neighborColors = new List<int>();

            // Check adjacent cells
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    neighborColors.Add(board[nx, ny]);
                }
            }

            if (neighborColors.Count > 0 && deterministicRandom.Range(0f, 1f) < 0.7f)
            {
                // Match a neighbor color
                return neighborColors[deterministicRandom.Range(0, neighborColors.Count)];
            }

            // Random color
            return deterministicRandom.Range(0, colorCount);
        }

        /// <summary>
        /// Generate pocket items for level start
        /// </summary>
        public int[] GeneratePocketItems(int pocketCount, int seed)
        {
            deterministicRandom.SetSeed(seed);
            int[] pockets = new int[pocketCount];

            for (int i = 0; i < pocketCount; i++)
            {
                pockets[i] = deterministicRandom.Range(0, colorCount);
            }

            return pockets;
        }

        /// <summary>
        /// Get difficulty rating for a level (1-10)
        /// </summary>
        public int GetDifficultyRating(LevelDef level)
        {
            float difficultyScore = 0f;

            // Factor in spawn interval (faster = harder)
            difficultyScore += (2.0f - level.spawnInterval) * 2f;

            // Factor in conveyor speed
            difficultyScore += level.conveyorSpeed * 1.5f;

            // Factor in target progress
            difficultyScore += level.targetProgress * 0.1f;

            // Factor in max spawn
            difficultyScore += (level.maxSpawn - 30) * 0.05f;

            return Mathf.Clamp(Mathf.RoundToInt(difficultyScore), 1, 10);
        }

        /// <summary>
        /// Check if level matches difficulty requirements
        /// </summary>
        public bool IsDifficultyAppropriate(LevelDef level, int targetDifficulty)
        {
            int actualDifficulty = GetDifficultyRating(level);
            int diff = Mathf.Abs(actualDifficulty - targetDifficulty);
            
            // Allow 2 levels of variance
            return diff <= 2;
        }
    }

    #region Level Generation Events

    public struct LevelGeneratedEvent
    {
        public LevelDef Level;
        public int Seed;
    }

    public struct LevelGenerationFailedEvent
    {
        public string Reason;
        public int RequestedDifficulty;
    }

    #endregion
}
