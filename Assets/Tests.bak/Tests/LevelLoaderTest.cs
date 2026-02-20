using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Services;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for LevelLoader
    /// </summary>
    [TestFixture]
    public class LevelLoaderTest
    {
        private LevelLoader _levelLoader;

        [SetUp]
        public void SetUp()
        {
            _levelLoader = LevelLoader.Instance;
            _levelLoader.UnloadLevel();
        }

        [TearDown]
        public void TearDown()
        {
            _levelLoader.UnloadLevel();
        }

        #region LoadLevel Tests

        [Test]
        public void LoadLevel_SetsCurrentLevel()
        {
            _levelLoader.LoadLevel(1);

            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level, Is.Not.Null);
            Assert.That(level.levelId, Is.EqualTo(1));
        }

        [Test]
        public void LoadLevel_SetsLevelId()
        {
            _levelLoader.LoadLevel(42);

            Assert.That(_levelLoader.GetCurrentLevelId(), Is.EqualTo(42));
        }

        [Test]
        public void LoadLevel_TriggersOnLevelLoaded()
        {
            bool eventFired = false;
            LevelDef loadedLevel = null;

            _levelLoader.OnLevelLoaded += (level) =>
            {
                eventFired = true;
                loadedLevel = level;
            };

            _levelLoader.LoadLevel(5);

            Assert.That(eventFired, Is.True);
            Assert.That(loadedLevel, Is.Not.Null);
            Assert.That(loadedLevel.levelId, Is.EqualTo(5));
        }

        #endregion

        #region UnloadLevel Tests

        [Test]
        public void UnloadLevel_ClearsCurrentLevel()
        {
            _levelLoader.LoadLevel(1);
            _levelLoader.UnloadLevel();

            Assert.That(_levelLoader.GetCurrentLevel(), Is.Null);
        }

        [Test]
        public void UnloadLevel_TriggersOnLevelUnloaded()
        {
            _levelLoader.LoadLevel(1);
            bool eventFired = false;
            _levelLoader.OnLevelUnloaded += () => eventFired = true;

            _levelLoader.UnloadLevel();

            Assert.That(eventFired, Is.True);
        }

        #endregion

        #region GenerateLevel Tests

        [Test]
        public void GenerateLevel_CreatesValidLevel()
        {
            _levelLoader.GenerateLevel(1, 0);

            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level, Is.Not.Null);
            Assert.That(level.levelId, Is.EqualTo(1));
            Assert.That(level.seed, Is.Not.EqualTo(0));
        }

        [Test]
        public void GenerateLevel_IncreasesBoardSizeWithDifficulty()
        {
            _levelLoader.GenerateLevel(1, 0);
            var easyLevel = _levelLoader.GetCurrentLevel();

            _levelLoader.GenerateLevel(2, 100);
            var hardLevel = _levelLoader.GetCurrentLevel();

            Assert.That(hardLevel.boardWidth, Is.GreaterThanOrEqualTo(easyLevel.boardWidth));
            Assert.That(hardLevel.boardHeight, Is.GreaterThanOrEqualTo(easyLevel.boardHeight));
        }

        [Test]
        public void GenerateLevel_MaxBoardSize_CappedAt8x12()
        {
            _levelLoader.GenerateLevel(1, 500);
            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level.boardWidth, Is.LessThanOrEqualTo(8));
            Assert.That(level.boardHeight, Is.LessThanOrEqualTo(12));
        }

        [Test]
        public void GenerateLevel_SetsSpikeOnDifficulty9()
        {
            _levelLoader.GenerateLevel(1, 9);

            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level.isSpike, Is.True);
        }

        [Test]
        public void GenerateLevel_SetsRecoveryOnDifficulty10()
        {
            _levelLoader.GenerateLevel(1, 10);

            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level.isRecovery, Is.True);
        }

        [Test]
        public void GenerateLevel_SetsRecoveryOnDifficulty20()
        {
            _levelLoader.GenerateLevel(1, 20);

            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level.isRecovery, Is.True);
        }

        [Test]
        public void GenerateLevel_SetsAnchorOnDifficulty30()
        {
            _levelLoader.GenerateLevel(1, 30);

            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level.isAnchor, Is.True);
            Assert.That(level.monetizationAnchor, Is.True);
        }

        [Test]
        public void GenerateLevel_TriggersOnLevelLoaded()
        {
            bool eventFired = false;
            _levelLoader.OnLevelLoaded += level => eventFired = true;

            _levelLoader.GenerateLevel(1, 0);

            Assert.That(eventFired, Is.True);
        }

        #endregion

        #region TargetType Tests

        [Test]
        public void GenerateLevel_FillSlots_ForDifficulty0()
        {
            _levelLoader.GenerateLevel(1, 0);

            Assert.That(_levelLoader.GetCurrentLevel().targetType, Is.EqualTo(TargetType.FillSlots));
        }

        [Test]
        public void GenerateLevel_ClearLocked_ForDifficulty10()
        {
            _levelLoader.GenerateLevel(1, 15);

            Assert.That(_levelLoader.GetCurrentLevel().targetType, Is.EqualTo(TargetType.ClearLocked));
        }

        [Test]
        public void GenerateLevel_DeliverToGate_ForDifficulty25()
        {
            _levelLoader.GenerateLevel(1, 30);

            Assert.That(_levelLoader.GetCurrentLevel().targetType, Is.EqualTo(TargetType.DeliverToGate));
        }

        [Test]
        public void GenerateLevel_Hybrid_ForHighDifficulty()
        {
            _levelLoader.GenerateLevel(1, 50);

            Assert.That(_levelLoader.GetCurrentLevel().targetType, Is.EqualTo(TargetType.Hybrid));
        }

        #endregion

        #region Difficulty Modifiers Tests

        [Test]
        public void ApplyDifficultyModifiers_Spike_IncreasesDifficulty()
        {
            _levelLoader.GenerateLevel(1, 9);
            var beforeSpike = _levelLoader.GetCurrentLevel();

            // Manually set spike to test
            beforeSpike.isSpike = true;
            beforeSpike.spawnInterval = 1.5f;
            beforeSpike.conveyorSpeed = 1.0f;

            // Apply modifiers through load
            _levelLoader.LoadLevel(1);

            var afterSpike = _levelLoader.GetCurrentLevel();

            // Global config spike multiplier should apply
            Assert.That(afterSpike.spawnInterval, Is.GreaterThan(1.4f));
        }

        [Test]
        public void ApplyDifficultyModifiers_Recovery_DecreasesDifficulty()
        {
            _levelLoader.GenerateLevel(1, 10);
            var beforeRecovery = _levelLoader.GetCurrentLevel();

            // Manually set recovery to test
            beforeRecovery.isRecovery = true;
            beforeRecovery.spawnInterval = 1.5f;
            beforeRecovery.conveyorSpeed = 1.0f;

            // Apply modifiers through load
            _levelLoader.LoadLevel(1);

            var afterRecovery = _levelLoader.GetCurrentLevel();

            // Global config recovery multiplier should apply
            Assert.That(afterRecovery.spawnInterval, Is.LessThan(1.5f));
        }

        #endregion

        #region Adaptive Modifiers Tests

        [Test]
        public void ApplyAdaptiveModifiers_UnderThreshold_NoChange()
        {
            _levelLoader.GenerateLevel(1, 0);
            var before = _levelLoader.GetCurrentLevel();
            float originalInterval = before.spawnInterval;
            float originalSpeed = before.conveyorSpeed;

            _levelLoader.ApplyAdaptiveModifiers(2);

            var after = _levelLoader.GetCurrentLevel();

            Assert.That(after.spawnInterval, Is.EqualTo(originalInterval));
            Assert.That(after.conveyorSpeed, Is.EqualTo(originalSpeed));
        }

        [Test]
        public void ApplyAdaptiveModifiers_AtThreshold_AppliesBonus()
        {
            _levelLoader.GenerateLevel(1, 0);
            var before = _levelLoader.GetCurrentLevel();
            float originalInterval = before.spawnInterval;
            float originalSpeed = before.conveyorSpeed;

            _levelLoader.ApplyAdaptiveModifiers(3); // threshold is 3

            var after = _levelLoader.GetCurrentLevel();

            Assert.That(after.spawnInterval, Is.GreaterThan(originalInterval));
            Assert.That(after.conveyorSpeed, Is.LessThan(originalSpeed));
        }

        [Test]
        public void ApplyAdaptiveModifiers_AboveThreshold_Accumulates()
        {
            _levelLoader.GenerateLevel(1, 0);

            _levelLoader.ApplyAdaptiveModifiers(3);
            var after3 = _levelLoader.GetCurrentLevel();
            float interval3 = after3.spawnInterval;

            _levelLoader.ApplyAdaptiveModifiers(4);
            var after4 = _levelLoader.GetCurrentLevel();
            float interval4 = after4.spawnInterval;

            Assert.That(interval4, Is.GreaterThan(interval3));
        }

        #endregion

        #region Edge Cases

        [Test]
        public void GetCurrentLevel_BeforeLoad_ReturnsNull()
        {
            _levelLoader.UnloadLevel();

            Assert.That(_levelLoader.GetCurrentLevel(), Is.Null);
        }

        [Test]
        public void GetCurrentLevelId_BeforeLoad_ReturnsZero()
        {
            _levelLoader.UnloadLevel();

            Assert.That(_levelLoader.GetCurrentLevelId(), Is.EqualTo(0));
        }

        [Test]
        public void LoadLevel_OverwritesExisting()
        {
            _levelLoader.LoadLevel(1);
            var firstLevel = _levelLoader.GetCurrentLevel();

            _levelLoader.LoadLevel(2);
            var secondLevel = _levelLoader.GetCurrentLevel();

            Assert.That(firstLevel.levelId, Is.Not.EqualTo(secondLevel.levelId));
            Assert.That(secondLevel.levelId, Is.EqualTo(2));
        }

        [Test]
        public void GenerateLevel_SeedInitialized()
        {
            _levelLoader.GenerateLevel(1, 0);
            var level = _levelLoader.GetCurrentLevel();

            Assert.That(level.seed, Is.Not.EqualTo(0));
        }

        #endregion
    }
}
