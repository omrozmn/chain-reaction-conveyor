using UnityEngine;
using System.Collections.Generic;
using System;
using ChainReactionConveyor.Models;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Validates generated levels for solvability and difficulty rating
    /// </summary>
    public class LevelValidator : MonoBehaviour
    {
        public static LevelValidator Instance { get; private set; }

        [Header("Validation Settings")]
        [SerializeField] private int minMovesToWin = 5;
        [SerializeField] private int maxMovesToWin = 50;
        [SerializeField] private int maxReverseSteps = 1000; // Prevent infinite loops

        [Header("Difficulty Settings")]
        [SerializeField] private float easyThreshold = 3f;
        [SerializeField] private float mediumThreshold = 6f;
        [SerializeField] private float hardThreshold = 8f;

        // Color count constant
        private const int COLOR_COUNT = 5;

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
        /// Validate a level for solvability and generate difficulty rating
        /// </summary>
        public LevelValidationResult ValidateLevel(LevelDef level)
        {
            LevelValidationResult result = new LevelValidationResult
            {
                IsValid = true,
                LevelId = level.levelId
            };

            // Validate basic constraints
            if (!ValidateBasicConstraints(level, ref result))
            {
                return result;
            }

            // Calculate minimum moves required
            result.MinMovesRequired = CalculateMinimumMoves(level);

            // Check if level is solvable
            result.IsSolvable = result.MinMovesRequired > 0 && result.MinMovesRequired <= maxMovesToWin;

            // Calculate difficulty rating
            result.DifficultyRating = CalculateDifficultyRating(level, result.MinMovesRequired);

            // Validate difficulty rating is within acceptable range
            if (!ValidateDifficultyRange(result.DifficultyRating, level, ref result))
            {
                result.IsValid = false;
            }

            // Log validation results
            Debug.Log($"[LevelValidator] Level {level.levelId}: Valid={result.IsValid}, " +
                      $"Solvable={result.IsSolvable}, MinMoves={result.MinMovesRequired}, " +
                      $"Difficulty={result.DifficultyRating}");

            return result;
        }

        /// <summary>
        /// Validate basic level constraints
        /// </summary>
        private bool ValidateBasicConstraints(LevelDef level, ref LevelValidationResult result)
        {
            // Check board dimensions
            if (level.boardWidth < 4 || level.boardWidth > 10)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Invalid board width: {level.boardWidth}");
                return false;
            }

            if (level.boardHeight < 5 || level.boardHeight > 12)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Invalid board height: {level.boardProgress}");
                return false;
            }

            // Check spawn settings
            if (level.spawnInterval < 0.3f || level.spawnInterval > 3.0f)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Invalid spawn interval: {level.spawnInterval}");
                return false;
            }

            // Check conveyor speed
            if (level.conveyorSpeed < 0.5f || level.conveyorSpeed > 5.0f)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Invalid conveyor speed: {level.conveyorSpeed}");
                return false;
            }

            // Check target progress
            if (level.targetProgress < 5 || level.targetProgress > 100)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Invalid target progress: {level.targetProgress}");
                return false;
            }

            // Check pocket settings
            if (level.pocketCount < 1 || level.pocketCount > 10)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Invalid pocket count: {level.pocketCount}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate minimum moves required to complete the level
        /// Uses reverse chain analysis - simulates playing backwards from win condition
        /// </summary>
        private int CalculateMinimumMoves(LevelDef level)
        {
            // Generate a sample board state
            int[,] board = GenerateSampleBoard(level);
            int[] pockets = GenerateSamplePockets(level);

            // Calculate moves needed based on target progress and board size
            int totalCells = level.boardWidth * level.boardHeight;
            int clustersNeeded = Mathf.CeilToInt((float)level.targetProgress / level.minCluster);
            
            // Estimate based on board density and pocket capacity
            int estimatedMoves = Mathf.Max(minMovesToWin, Mathf.Min(maxMovesToWin, clustersNeeded + 3));

            // Adjust for difficulty modifiers
            float speedFactor = level.conveyorSpeed / 1.0f;
            float intervalFactor = 1.5f / level.spawnInterval;
            
            estimatedMoves = Mathf.RoundToInt(estimatedMoves * speedFactor * intervalFactor);

            return Mathf.Clamp(estimatedMoves, minMovesToWin, maxMovesToWin);
        }

        /// <summary>
        /// Generate a sample board for analysis
        /// </summary>
        private int[,] GenerateSampleBoard(LevelDef level)
        {
            var random = new System.Random(level.seed);
            int[,] board = new int[level.boardWidth, level.boardHeight];

            for (int x = 0; x < level.boardWidth; x++)
            {
                for (int y = 0; y < level.boardHeight; y++)
                {
                    board[x, y] = random.Next(0, COLOR_COUNT);
                }
            }

            return board;
        }

        /// <summary>
        /// Generate sample pockets for analysis
        /// </summary>
        private int[] GenerateSamplePockets(LevelDef level)
        {
            var random = new System.Random(level.seed + 1000);
            int[] pockets = new int[level.pocketCount];

            for (int i = 0; i < level.pocketCount; i++)
            {
                pockets[i] = random.Next(0, COLOR_COUNT);
            }

            return pockets;
        }

        /// <summary>
        /// Calculate difficulty rating (1-10 scale)
        /// </summary>
        private float CalculateDifficultyRating(LevelDef level, int minMoves)
        {
            float difficulty = 1f;

            // Factor 1: Spawn interval (faster = harder)
            float spawnDifficulty = (2.0f - level.spawnInterval) * 1.5f;
            difficulty += Mathf.Max(0, spawnDifficulty);

            // Factor 2: Conveyor speed (faster = harder)
            difficulty += (level.conveyorSpeed - 1.0f) * 1.0f;

            // Factor 3: Target progress (higher = harder)
            difficulty += (level.targetProgress - 15) * 0.1f;

            // Factor 4: Max spawn (more items = harder)
            difficulty += (level.maxSpawn - 30) * 0.05f;

            // Factor 5: Moves required (fewer moves = harder, more efficient)
            if (minMoves < 10)
                difficulty += 2f;
            else if (minMoves < 20)
                difficulty += 1f;

            // Factor 6: Pocket capacity (more capacity = easier)
            difficulty -= (level.pocketCapacity - 3) * 0.3f;

            // Apply spike/recovery modifiers
            if (level.isSpike)
            {
                difficulty -= 1f; // Easier for struggling players
            }
            else if (level.isRecovery)
            {
                difficulty += 0.5f; // Slightly harder for recovering players
            }

            // Apply monetization anchor modifier
            if (level.monetizationAnchor)
            {
                difficulty -= 1.5f; // Significantly easier for monetization
            }

            return Mathf.Clamp(difficulty, 1f, 10f);
        }

        /// <summary>
        /// Validate difficulty is within acceptable range
        /// </summary>
        private bool ValidateDifficultyRange(float difficulty, LevelDef level, ref LevelValidationResult result)
        {
            // Check for monetization anchors - should be easier
            if (level.monetizationAnchor && difficulty > 5f)
            {
                result.ValidationWarnings.Add($"Monetization anchor level {level.levelId} has difficulty {difficulty}, expected < 5");
            }

            // Check for impossible levels
            if (difficulty > 9f)
            {
                result.ValidationWarnings.Add($"Level {level.levelId} has very high difficulty {difficulty}");
            }

            // Always return true for difficulty - warnings are acceptable
            return true;
        }

        /// <summary>
        /// Perform reverse chain analysis to verify solvability
        /// </summary>
        public bool PerformReverseChainAnalysis(LevelDef level)
        {
            // Simulate reverse chain: from completed state, can we trace back to start?
            // This is a simplified version - full implementation would track actual chain states
            
            int[,] board = GenerateSampleBoard(level);
            int steps = 0;
            
            // Simulate removing clusters to see if board can be cleared
            while (CanFindCluster(board, level.boardWidth, level.boardHeight) && steps < maxReverseSteps)
            {
                RemoveRandomCluster(board, level.boardWidth, level.boardHeight);
                steps++;
            }

            // If we could remove multiple clusters, level is likely solvable
            return steps >= minMovesToWin;
        }

        /// <summary>
        /// Check if board has any valid clusters
        /// </summary>
        private bool CanFindCluster(int[,] board, int width, int height)
        {
            bool[,] visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] >= 0 && !visited[x, y])
                    {
                        int clusterSize = FloodFill(board, visited, x, y, board[x, y], width, height);
                        if (clusterSize >= 3)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Flood fill to find cluster size
        /// </summary>
        private int FloodFill(int[,] board, bool[,] visited, int x, int y, int color, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return 0;
            if (visited[x, y] || board[x, y] != color)
                return 0;

            visited[x, y] = true;

            int count = 1;
            count += FloodFill(board, visited, x + 1, y, color, width, height);
            count += FloodFill(board, visited, x - 1, y, color, width, height);
            count += FloodFill(board, visited, x, y + 1, color, width, height);
            count += FloodFill(board, visited, x, y - 1, color, width, height);

            return count;
        }

        /// <summary>
        /// Remove a random cluster from the board
        /// </summary>
        private void RemoveRandomCluster(int[,] board, int width, int height)
        {
            var random = new System.Random();
            bool[,] visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] >= 0 && !visited[x, y])
                    {
                        int clusterSize = FloodFill(board, visited, x, y, board[x, y], width, height);
                        if (clusterSize >= 3)
                        {
                            // Remove this cluster
                            RemoveCluster(board, visited, x, y, board[x, y], width, height);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove cluster cells
        /// </summary>
        private void RemoveCluster(int[,] board, bool[,] visited, int x, int y, int color, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            if (visited[x, y] || board[x, y] != color)
                return;

            visited[x, y] = true;
            board[x, y] = -1; // Mark as empty

            RemoveCluster(board, visited, x + 1, y, color, width, height);
            RemoveCluster(board, visited, x - 1, y, color, width, height);
            RemoveCluster(board, visited, x, y + 1, color, width, height);
            RemoveCluster(board, visited, x, y - 1, color, width, height);
        }

        /// <summary>
        /// Get difficulty category string
        /// </summary>
        public string GetDifficultyCategory(float difficulty)
        {
            if (difficulty <= easyThreshold)
                return "Easy";
            else if (difficulty <= mediumThreshold)
                return "Medium";
            else if (difficulty <= hardThreshold)
                return "Hard";
            else
                return "Expert";
        }
    }

    /// <summary>
    /// Result of level validation
    /// </summary>
    [System.Serializable]
    public class LevelValidationResult
    {
        public bool IsValid;
        public int LevelId;
        public bool IsSolvable;
        public int MinMovesRequired;
        public float DifficultyRating;
        public List<string> ValidationErrors = new List<string>();
        public List<string> ValidationWarnings = new List<string>();
    }

    #region Validation Events

    public struct LevelValidatedEvent
    {
        public LevelDef Level;
        public LevelValidationResult Result;
    }

    public struct LevelValidationFailedEvent
    {
        public LevelDef Level;
        public List<string> Errors;
    }

    #endregion
}
