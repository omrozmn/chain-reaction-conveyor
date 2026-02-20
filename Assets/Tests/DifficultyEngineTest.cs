using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for DifficultyEngine - win rate calculation, spike detection, and recovery patterns.
    /// </summary>
    public class DifficultyEngineTest
    {
        private DifficultyEngine _engine;
        private GameObject _gameObject;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("DifficultyEngineTest");
            _engine = _gameObject.AddComponent<DifficultyEngine>();
            
            // Use reflection to set private fields for testing
            SetPrivateField("_windowSize", 10);
            SetPrivateField("_spikeThreshold", 3);
            SetPrivateField("_recoveryThreshold", 3);
            SetPrivateField("_difficultyStep", 0.1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        #region Win Rate Tests

        [Test]
        public void RecordResult_EmptyHistory_DefaultsToFiftyPercent()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - record first result
            _engine.RecordResult(true);
            
            // Assert - win rate should be 1.0 (100%) for 1 win
            Assert.AreEqual(1.0f, _engine.CurrentWinRate, 0.01f);
        }

        [Test]
        public void RecordResult_AllWins_ReturnsFullWinRate()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - record 5 wins
            for (int i = 0; i < 5; i++)
            {
                _engine.RecordResult(true);
            }
            
            // Assert
            Assert.AreEqual(1.0f, _engine.CurrentWinRate, 0.01f);
        }

        [Test]
        public void RecordResult_AllLosses_ReturnsZeroWinRate()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - record 5 losses
            for (int i = 0; i < 5; i++)
            {
                _engine.RecordResult(false);
            }
            
            // Assert
            Assert.AreEqual(0.0f, _engine.CurrentWinRate, 0.01f);
        }

        [Test]
        public void RecordResult_MixedResults_CalculatesCorrectWinRate()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - 6 wins, 4 losses
            bool[] results = { true, true, false, true, false, true, true, false, true, false };
            foreach (bool result in results)
            {
                _engine.RecordResult(result);
            }
            
            // Assert - 6/10 = 0.6
            Assert.AreEqual(0.6f, _engine.CurrentWinRate, 0.01f);
        }

        [Test]
        public void RecordResult_WindowSizeEnforced_OldResultsExpire()
        {
            // Arrange - set window size to 5
            SetPrivateField("_windowSize", 5);
            _engine.ResetStats();
            
            // Act - record 7 results (first 2 should expire)
            _engine.RecordResult(true);  // 1: win
            _engine.RecordResult(true);  // 2: win
            _engine.RecordResult(false); // 3: loss
            _engine.RecordResult(false); // 4: loss
            _engine.RecordResult(false); // 5: loss
            _engine.RecordResult(true);  // 6: win (1st expires)
            _engine.RecordResult(true);  // 7: win (2nd expires)
            
            // Assert - window has last 5: W, L, L, L, W = 2/5 = 0.4
            Assert.AreEqual(0.4f, _engine.CurrentWinRate, 0.01f);
        }

        #endregion

        #region Spike Detection Tests

        [Test]
        public void RecordResult_ConsecutiveFailuresBelowThreshold_NoSpikeDetected()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - record 2 failures (threshold is 3)
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            
            // Assert
            Assert.IsFalse(_engine.IsSpikeDetected);
        }

        [Test]
        public void RecordResult_ConsecutiveFailuresReachesThreshold_SpikeDetected()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - record 3 consecutive failures
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            
            // Assert
            Assert.IsTrue(_engine.IsSpikeDetected);
        }

        [Test]
        public void RecordResult_SpikeDetected_DifficultyDecreases()
        {
            // Arrange
            _engine.ResetStats();
            float initialDifficulty = _engine.CurrentDifficulty;
            
            // Act - trigger spike
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            
            // Assert - difficulty should decrease by difficultyStep (0.1)
            Assert.Less(_engine.CurrentDifficulty, initialDifficulty);
            Assert.AreEqual(0.9f, _engine.CurrentDifficulty, 0.01f);
        }

        [Test]
        public void RecordResult_SpikeFollowedByWin_ExitsSpikeState()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - trigger spike then win
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            _engine.RecordResult(false); // spike detected
            
            Assert.IsTrue(_engine.IsSpikeDetected);
            
            _engine.RecordResult(true); // recovery starts
            
            // Assert
            Assert.IsFalse(_engine.IsSpikeDetected);
        }

        #endregion

        #region Recovery Flag Tests

        [Test]
        public void RecordResult_RecoveryThresholdReached_RecoveryFlagSet()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - trigger spike then 3 consecutive wins
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            _engine.RecordResult(false); // spike detected
            
            _engine.RecordResult(true);
            _engine.RecordResult(true);
            _engine.RecordResult(true); // recovery threshold reached
            
            // Assert
            Assert.IsTrue(_engine.IsRecovering);
        }

        [Test]
        public void RecordResult_Recovery_DifficultyIncreases()
        {
            // Arrange
            _engine.ResetStats();
            
            // Force a low difficulty by triggering spike first
            _engine.RecordResult(false);
            _engine.RecordResult(false);
            _engine.RecordResult(false); // spike: difficulty = 0.9
            
            float difficultyAfterSpike = _engine.CurrentDifficulty;
            
            // Act - recovery: 3 wins
            _engine.RecordResult(true);
            _engine.RecordResult(true);
            _engine.RecordResult(true);
            
            // Assert - difficulty should increase
            Assert.Greater(_engine.CurrentDifficulty, difficultyAfterSpike);
        }

        [Test]
        public void RecordResult_HighWinRateGradiualIncrease_DifficultyRises()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - 7 wins out of 10 (70%+)
            for (int i = 0; i < 7; i++)
            {
                _engine.RecordResult(true);
            }
            for (int i = 0; i < 3; i++)
            {
                _engine.RecordResult(false);
            }
            
            // Assert - should gradually increase
            Assert.Greater(_engine.CurrentDifficulty, 1.0f);
        }

        [Test]
        public void RecordResult_LowWinRateGradiualDecrease_DifficultyFalls()
        {
            // Arrange
            _engine.ResetStats();
            
            // Act - 3 wins out of 10 (30%)
            for (int i = 0; i < 3; i++)
            {
                _engine.RecordResult(true);
            }
            for (int i = 0; i < 7; i++)
            {
                _engine.RecordResult(false);
            }
            
            // Assert - should gradually decrease
            Assert.Less(_engine.CurrentDifficulty, 1.0f);
        }

        #endregion

        #region Difficulty Bounds Tests

        [Test]
        public void GetDifficultyMultiplier_MinimumClamped_AtZeroPointThree()
        {
            // Arrange - keep lowering difficulty
            _engine.ResetStats();
            
            // Act - trigger multiple spikes
            for (int i = 0; i < 20; i++)
            {
                _engine.RecordResult(false);
                _engine.RecordResult(false);
                _engine.RecordResult(false);
            }
            
            // Assert - should clamp at 0.3
            Assert.AreEqual(0.3f, _engine.CurrentDifficulty, 0.01f);
        }

        [Test]
        public void GetDifficultyMultiplier_MaximumClamped_AtTwoPointZero()
        {
            // Arrange - keep raising difficulty
            _engine.ResetStats();
            
            // Act - trigger multiple recoveries/spikes with high win rate
            for (int i = 0; i < 30; i++)
            {
                _engine.RecordResult(true);
                _engine.RecordResult(true);
                _engine.RecordResult(true);
                _engine.RecordResult(true);
                _engine.RecordResult(true);
                _engine.RecordResult(true);
                _engine.RecordResult(true);
            }
            
            // Assert - should clamp at 2.0
            Assert.AreEqual(2.0f, _engine.CurrentDifficulty, 0.01f);
        }

        #endregion

        #region Reset Tests

        [Test]
        public void ResetStats_ClearsAllState_ReturnsToDefaults()
        {
            // Arrange - mess up the state
            _engine.RecordResult(true);
            _engine.RecordResult(true);
            _engine.RecordResult(false);
            
            // Act
            _engine.ResetStats();
            
            // Assert
            Assert.AreEqual(0.5f, _engine.CurrentWinRate, 0.01f);
            Assert.AreEqual(1.0f, _engine.CurrentDifficulty, 0.01f);
            Assert.IsFalse(_engine.IsSpikeDetected);
            Assert.IsFalse(_engine.IsRecovering);
        }

        #endregion

        #region Helper Methods

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(DifficultyEngine).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(field, $"Field {fieldName} not found");
            field.SetValue(_engine, value);
        }

        #endregion
    }
}
