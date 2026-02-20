using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Systems;
using ChainReactionConveyor.Models;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for LevelValidator - validates generated levels
    /// </summary>
    [TestFixture]
    public class LevelValidatorTest
    {
        private GameObject _gameObject;
        private LevelValidator _levelValidator;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("LevelValidatorTest");
            _levelValidator = _gameObject.AddComponent<LevelValidator>();
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
            Assert.That(LevelValidator.Instance, Is.EqualTo(_levelValidator));
        }

        [Test]
        public void Instance_Singleton_PreventsDuplicates()
        {
            var otherGameObject = new GameObject("OtherLevelValidator");
            var otherValidator = otherGameObject.AddComponent<LevelValidator>();
            
            // Should destroy duplicate
            Assert.That(LevelValidator.Instance, Is.EqualTo(_levelValidator));
            
            GameObject.DestroyImmediate(otherGameObject);
        }

        #endregion

        #region Basic Validation Tests

        [Test]
        public void ValidateLevel_ValidLevel_ReturnsIsValidTrue()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateLevel_ValidLevel_ReturnsCorrectLevelId()
        {
            // Arrange
            var level = CreateValidLevel();
            level.levelId = 42;
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.LevelId, Is.EqualTo(42));
        }

        [Test]
        public void ValidateLevel_InvalidBoardWidth_AddsError()
        {
            // Arrange
            var level = CreateValidLevel();
            level.boardWidth = 2; // Too small
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationErrors, Is.Not.Empty);
            StringAssert.Contains("board width", result.ValidationErrors[0]);
        }

        [Test]
        public void ValidateLevel_InvalidBoardHeight_AddsError()
        {
            // Arrange
            var level = CreateValidLevel();
            level.boardHeight = 20; // Too large
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationErrors, Is.Not.Empty);
            StringAssert.Contains("board height", result.ValidationErrors[0]);
        }

        [Test]
        public void ValidateLevel_InvalidSpawnInterval_AddsError()
        {
            // Arrange
            var level = CreateValidLevel();
            level.spawnInterval = 0.1f; // Too fast
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationErrors, Is.Not.Empty);
            StringAssert.Contains("spawn interval", result.ValidationErrors[0]);
        }

        [Test]
        public void ValidateLevel_InvalidConveyorSpeed_AddsError()
        {
            // Arrange
            var level = CreateValidLevel();
            level.conveyorSpeed = 10f; // Too fast
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationErrors, Is.Not.Empty);
            StringAssert.Contains("conveyor speed", result.ValidationErrors[0]);
        }

        [Test]
        public void ValidateLevel_InvalidTargetProgress_AddsError()
        {
            // Arrange
            var level = CreateValidLevel();
            level.targetProgress = 2; // Too low
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationErrors, Is.Not.Empty);
            StringAssert.Contains("target progress", result.ValidationErrors[0]);
        }

        [Test]
        public void ValidateLevel_InvalidPocketCount_AddsError()
        {
            // Arrange
            var level = CreateValidLevel();
            level.pocketCount = 0; // Invalid
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ValidationErrors, Is.Not.Empty);
            StringAssert.Contains("pocket count", result.ValidationErrors[0]);
        }

        #endregion

        #region Solvability Tests

        [Test]
        public void ValidateLevel_ValidLevel_IsSolvableTrue()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsSolvable, Is.True);
        }

        [Test]
        public void ValidateLevel_HasMinMovesRequired()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.MinMovesRequired, Is.GreaterThan(0));
        }

        [Test]
        public void ValidateLevel_MinMovesWithinRange()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert - should be within configured min/max
            Assert.That(result.MinMovesRequired, Is.GreaterThanOrEqualTo(5));
            Assert.That(result.MinMovesRequired, Is.LessThanOrEqualTo(50));
        }

        #endregion

        #region Difficulty Rating Tests

        [Test]
        public void ValidateLevel_ReturnsDifficultyRating()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.DifficultyRating, Is.GreaterThan(0));
        }

        [Test]
        public void ValidateLevel_DifficultyRatingWithinRange()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.DifficultyRating, Is.GreaterThanOrEqualTo(1f));
            Assert.That(result.DifficultyRating, Is.LessThanOrEqualTo(10f));
        }

        [Test]
        public void ValidateLevel_MonetizationAnchor_LowersDifficulty()
        {
            // Arrange
            var normalLevel = CreateValidLevel();
            var monetizedLevel = CreateValidLevel();
            monetizedLevel.monetizationAnchor = true;
            
            // Act
            var normalResult = _levelValidator.ValidateLevel(normalLevel);
            var monetizedResult = _levelValidator.ValidateLevel(monetizedLevel);
            
            // Assert
            Assert.That(monetizedResult.DifficultyRating, Is.LessThan(normalResult.DifficultyRating));
        }

        [Test]
        public void ValidateLevel_SpikeLevel_HasWarning()
        {
            // Arrange
            var level = CreateValidLevel();
            level.isSpike = true;
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.ValidationWarnings, Is.Not.Empty);
        }

        #endregion

        #region Reverse Chain Analysis Tests

        [Test]
        public void PerformReverseChainAnalysis_ValidLevel_ReturnsTrue()
        {
            // Arrange
            var level = CreateValidLevel();
            
            // Act
            bool result = _levelValidator.PerformReverseChainAnalysis(level);
            
            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region Difficulty Category Tests

        [Test]
        public void GetDifficultyCategory_EasyLevel_ReturnsEasy()
        {
            // Act
            string category = _levelValidator.GetDifficultyCategory(2f);
            
            // Assert
            Assert.That(category, Is.EqualTo("Easy"));
        }

        [Test]
        public void GetDifficultyCategory_MediumLevel_ReturnsMedium()
        {
            // Act
            string category = _levelValidator.GetDifficultyCategory(5f);
            
            // Assert
            Assert.That(category, Is.EqualTo("Medium"));
        }

        [Test]
        public void GetDifficultyCategory_HardLevel_ReturnsHard()
        {
            // Act
            string category = _levelValidator.GetDifficultyCategory(7f);
            
            // Assert
            Assert.That(category, Is.EqualTo("Hard"));
        }

        [Test]
        public void GetDifficultyCategory_ExpertLevel_ReturnsExpert()
        {
            // Act
            string category = _levelValidator.GetDifficultyCategory(9f);
            
            // Assert
            Assert.That(category, Is.EqualTo("Expert"));
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void ValidateLevel_MinimumValidBoard_ReturnsValid()
        {
            // Arrange - minimum valid dimensions
            var level = CreateValidLevel();
            level.boardWidth = 4;
            level.boardHeight = 5;
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateLevel_MaximumValidBoard_ReturnsValid()
        {
            // Arrange - maximum valid dimensions
            var level = CreateValidLevel();
            level.boardWidth = 10;
            level.boardHeight = 12;
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateLevel_MinimumSpawnInterval_ReturnsValid()
        {
            // Arrange
            var level = CreateValidLevel();
            level.spawnInterval = 0.3f; // Minimum allowed
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateLevel_MaximumSpawnInterval_ReturnsValid()
        {
            // Arrange
            var level = CreateValidLevel();
            level.spawnInterval = 3.0f; // Maximum allowed
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateLevel_MinimumTargetProgress_ReturnsValid()
        {
            // Arrange
            var level = CreateValidLevel();
            level.targetProgress = 5; // Minimum allowed
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ValidateLevel_MaximumTargetProgress_ReturnsValid()
        {
            // Arrange
            var level = CreateValidLevel();
            level.targetProgress = 100; // Maximum allowed
            
            // Act
            var result = _levelValidator.ValidateLevel(level);
            
            // Assert
            Assert.That(result.IsValid, Is.True);
        }

        #endregion

        #region Helper Methods

        private LevelDef CreateValidLevel()
        {
            return new LevelDef
            {
                levelId = 1,
                seed = 12345,
                boardWidth = 6,
                boardHeight = 8,
                pocketCount = 5,
                pocketCapacity = 3,
                maxSpawn = 30,
                minCluster = 3,
                spawnInterval = 1.5f,
                conveyorSpeed = 1.0f,
                targetProgress = 20,
                targetType = TargetType.FillSlots
            };
        }

        #endregion
    }
}
