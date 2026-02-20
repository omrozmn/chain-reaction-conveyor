using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Systems;
using ChainReactionConveyor.Models;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for LevelGenerator - procedural level generation
    /// </summary>
    [TestFixture]
    public class LevelGeneratorTest
    {
        private GameObject _gameObject;
        private LevelGenerator _levelGenerator;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("LevelGeneratorTest");
            _levelGenerator = _gameObject.AddComponent<LevelGenerator>();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(_gameObject);
        }

        #region Instance Tests

        [Test]
        public void Instance_IsSet_OnAwake()
        {
            Assert.That(LevelGenerator.Instance, Is.EqualTo(_levelGenerator));
        }

        [Test]
        public void Instance_Singleton_PreventsDuplicates()
        {
            var otherGameObject = new GameObject("OtherLevelGenerator");
            var otherGenerator = otherGameObject.AddComponent<LevelGenerator>();
            
            // Should destroy duplicate
            Assert.That(LevelGenerator.Instance, Is.EqualTo(_levelGenerator));
            
            GameObject.DestroyImmediate(otherGameObject);
        }

        #endregion

        #region Basic Generation Tests

        [Test]
        public void GenerateLevel_CreatesValidLevelDef()
        {
            // Act
            var level = _levelGenerator.GenerateLevel(5, 1);
            
            // Assert
            Assert.That(level, Is.Not.Null);
            Assert.That(level.levelId, Is.EqualTo(1));
            Assert.That(level.seed, Is.GreaterThan(0));
        }

        [Test]
        public void GenerateLevel_RespectsBoardDimensions()
        {
            // Act
            var level = _levelGenerator.GenerateLevel(5, 1);
            
            // Assert - default values
            Assert.That(level.boardWidth, Is.EqualTo(6));
            Assert.That(level.boardHeight, Is.EqualTo(8));
        }

        [Test]
        public void GenerateLevel_SetsDefaultPocketSettings()
        {
            // Act
            var level = _levelGenerator.GenerateLevel(5, 1);
            
            // Assert
            Assert.That(level.pocketCount, Is.EqualTo(5));
            Assert.That(level.pocketCapacity, Is.EqualTo(3));
        }

        [Test]
        public void GenerateLevel_SameSeed_ProducesSameLevel()
        {
            // Arrange
            int seed = 12345;
            
            // Act
            var level1 = _levelGenerator.GenerateLevel(5, 1, seed);
            var level2 = _levelGenerator.GenerateLevel(5, 1, seed);
            
            // Assert
            Assert.That(level1.seed, Is.EqualTo(level2.seed));
            Assert.That(level1.boardWidth, Is.EqualTo(level2.boardWidth));
            Assert.That(level1.boardHeight, Is.EqualTo(level2.boardHeight));
            Assert.That(level1.targetProgress, Is.EqualTo(level2.targetProgress));
        }

        [Test]
        public void GenerateLevel_DifferentSeeds_ProducesDifferentSeeds()
        {
            // Act
            var level1 = _levelGenerator.GenerateLevel(5, 1, 100);
            var level2 = _levelGenerator.GenerateLevel(5, 1, 200);
            
            // Assert
            Assert.That(level1.seed, Is.Not.EqualTo(level2.seed));
        }

        #endregion

        #region Difficulty Scaling Tests

        [Test]
        public void GenerateLevel_HigherDifficulty_IncreasesTargetProgress()
        {
            // Act
            var easyLevel = _levelGenerator.GenerateLevel(1, 1);
            var hardLevel = _levelGenerator.GenerateLevel(10, 1);
            
            // Assert
            Assert.That(hardLevel.targetProgress, Is.GreaterThan(easyLevel.targetProgress));
        }

        [Test]
        public void GenerateLevel_HigherDifficulty_IncreasesSpawnRate()
        {
            // Act
            var easyLevel = _levelGenerator.GenerateLevel(1, 1);
            var hardLevel = _levelGenerator.GenerateLevel(10, 1);
            
            // Assert - higher difficulty = lower spawn interval (faster)
            Assert.That(hardLevel.spawnInterval, Is.LessThan(easyLevel.spawnInterval));
        }

        [Test]
        public void GenerateLevel_HigherDifficulty_IncreasesConveyorSpeed()
        {
            // Act
            var easyLevel = _levelGenerator.GenerateLevel(1, 1);
            var hardLevel = _levelGenerator.GenerateLevel(10, 1);
            
            // Assert
            Assert.That(hardLevel.conveyorSpeed, Is.GreaterThan(easyLevel.conveyorSpeed));
        }

        [Test]
        public void GenerateLevel_HigherDifficulty_IncreasesMaxSpawn()
        {
            // Act
            var easyLevel = _levelGenerator.GenerateLevel(1, 1);
            var hardLevel = _levelGenerator.GenerateLevel(10, 1);
            
            // Assert
            Assert.That(hardLevel.maxSpawn, Is.GreaterThanOrEqualTo(easyLevel.maxSpawn));
        }

        [Test]
        public void GenerateLevel_LevelNumber_IncreasesTargetProgress()
        {
            // Act
            var level1 = _levelGenerator.GenerateLevel(5, 1);
            var level10 = _levelGenerator.GenerateLevel(5, 10);
            var level50 = _levelGenerator.GenerateLevel(5, 50);
            
            // Assert
            Assert.That(level10.targetProgress, Is.GreaterThan(level1.targetProgress));
            Assert.That(level50.targetProgress, Is.GreaterThan(level10.targetProgress));
        }

        #endregion

        #region Constraint Validation Tests

        [Test]
        public void GenerateLevel_InvalidSpawnInterval_AdjustedToMeetConstraints()
        {
            // Arrange - very high difficulty should clamp spawn interval
            var level = _levelGenerator.GenerateLevel(100, 1);
            
            // Assert
            Assert.That(level.spawnInterval, Is.GreaterThanOrEqualTo(0.3f));
            Assert.That(level.spawnInterval, Is.LessThanOrEqualTo(3.0f));
        }

        [Test]
        public void GenerateLevel_InvalidConveyorSpeed_AdjustedToMeetConstraints()
        {
            // Act
            var level = _levelGenerator.GenerateLevel(100, 1);
            
            // Assert
            Assert.That(level.conveyorSpeed, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(level.conveyorSpeed, Is.LessThanOrEqualTo(5.0f));
        }

        [Test]
        public void GenerateLevel_InvalidTargetProgress_AdjustedToMeetConstraints()
        {
            // Act
            var level = _levelGenerator.GenerateLevel(100, 1);
            
            // Assert
            Assert.That(level.targetProgress, Is.GreaterThanOrEqualTo(5));
            Assert.That(level.targetProgress, Is.LessThanOrEqualTo(100));
        }

        #endregion

        #region Difficulty Rating Tests

        [Test]
        public void GetDifficultyRating_ReturnsValueBetween1And10()
        {
            // Act
            var level = _levelGenerator.GenerateLevel(5, 1);
            int rating = _levelGenerator.GetDifficultyRating(level);
            
            // Assert
            Assert.That(rating, Is.GreaterThanOrEqualTo(1));
            Assert.That(rating, Is.LessThanOrEqualTo(10));
        }

        [Test]
        public void GetDifficultyRating_HigherForHarderLevels()
        {
            // Arrange
            var easyLevel = _levelGenerator.GenerateLevel(1, 1);
            var hardLevel = _levelGenerator.GenerateLevel(10, 1);
            
            // Act
            int easyRating = _levelGenerator.GetDifficultyRating(easyLevel);
            int hardRating = _levelGenerator.GetDifficultyRating(hardLevel);
            
            // Assert
            Assert.That(hardRating, Is.GreaterThan(easyRating));
        }

        [Test]
        public void IsDifficultyAppropriate_ReturnsTrue_WhenWithinRange()
        {
            // Arrange
            var level = _levelGenerator.GenerateLevel(5, 1);
            
            // Act & Assert
            Assert.That(_levelGenerator.IsDifficultyAppropriate(level, 5), Is.True);
            Assert.That(_levelGenerator.IsDifficultyAppropriate(level, 4), Is.True);
            Assert.That(_levelGenerator.IsDifficultyAppropriate(level, 6), Is.True);
        }

        [Test]
        public void IsDifficultyAppropriate_ReturnsFalse_WhenOutOfRange()
        {
            // Arrange
            var level = _levelGenerator.GenerateLevel(1, 1); // Easy level
            
            // Act & Assert - difficulty 8 is too far from actual difficulty
            Assert.That(_levelGenerator.IsDifficultyAppropriate(level, 8), Is.False);
        }

        #endregion

        #region Board Generation Tests

        [Test]
        public void GenerateInitialBoard_ReturnsValidArray()
        {
            // Act
            int[,] board = _levelGenerator.GenerateInitialBoard(6, 8, 123);
            
            // Assert
            Assert.That(board, Is.Not.Null);
            Assert.That(board.GetLength(0), Is.EqualTo(6));
            Assert.That(board.GetLength(1), Is.EqualTo(8));
        }

        [Test]
        public void GenerateInitialBoard_SameSeed_ProducesSameBoard()
        {
            // Act
            int[,] board1 = _levelGenerator.GenerateInitialBoard(6, 8, 999);
            int[,] board2 = _levelGenerator.GenerateInitialBoard(6, 8, 999);
            
            // Assert
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Assert.That(board1[x, y], Is.EqualTo(board2[x, y]));
                }
            }
        }

        [Test]
        public void GenerateInitialBoard_ContainsValidColorIndices()
        {
            // Act
            int[,] board = _levelGenerator.GenerateInitialBoard(6, 8, 123);
            
            // Assert - colors should be 0-4 (5 colors)
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Assert.That(board[x, y], Is.GreaterThanOrEqualTo(0));
                    Assert.That(board[x, y], Is.LessThan(5));
                }
            }
        }

        #endregion

        #region Pocket Generation Tests

        [Test]
        public void GeneratePocketItems_ReturnsCorrectCount()
        {
            // Act
            int[] pockets = _levelGenerator.GeneratePocketItems(5, 123);
            
            // Assert
            Assert.That(pockets.Length, Is.EqualTo(5));
        }

        [Test]
        public void GeneratePocketItems_SameSeed_ProducesSameItems()
        {
            // Act
            int[] pockets1 = _levelGenerator.GeneratePocketItems(5, 777);
            int[] pockets2 = _levelGenerator.GeneratePocketItems(5, 777);
            
            // Assert
            Assert.That(pockets1, Is.EqualTo(pockets2));
        }

        [Test]
        public void GeneratePocketItems_ContainsValidColorIndices()
        {
            // Act
            int[] pockets = _levelGenerator.GeneratePocketItems(5, 123);
            
            // Assert
            foreach (var color in pockets)
            {
                Assert.That(color, Is.GreaterThanOrEqualTo(0));
                Assert.That(color, Is.LessThan(5));
            }
        }

        #endregion

        #region Adaptive Level Tests

        [Test]
        public void GenerateAdaptiveLevel_CreatesLevel()
        {
            // Act
            var level = _levelGenerator.GenerateAdaptiveLevel(1);
            
            // Assert
            Assert.That(level, Is.Not.Null);
            Assert.That(level.levelId, Is.EqualTo(1));
        }

        #endregion
    }
}
