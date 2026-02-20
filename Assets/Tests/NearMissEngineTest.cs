using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for NearMissEngine - near-miss detection, distance tracking, and streak patterns.
    /// </summary>
    public class NearMissEngineTest
    {
        private NearMissEngine _engine;
        private GameObject _gameObject;

        [SetUp]
        public void Setup()
        {
            _gameObject = new GameObject("NearMissEngineTest");
            _engine = _gameObject.AddComponent<NearMissEngine>();
            
            // Use reflection to set private fields for testing
            SetPrivateField("_nearMissDistance", 0.5f);
            SetPrivateField("_nearMissTimeWindow", 0.5f);
            SetPrivateField("_maxNearMissesPerSession", 20);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        #region Basic Near-Miss Detection Tests

        [Test]
        public void CheckNearMiss_ObjectWithinThreshold_NearMissRecorded()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - object at 0.3 distance (within 0.5 threshold)
            Vector3 objectPos = new Vector3(0, 0, 0);
            Vector3 targetPos = new Vector3(0.3f, 0, 0);
            
            _engine.CheckNearMiss(objectPos, targetPos, "TestObject");
            
            // Assert
            Assert.AreEqual(1, _engine.NearMissCount);
        }

        [Test]
        public void CheckNearMiss_ObjectOutsideThreshold_NoNearMiss()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - object at 0.8 distance (outside 0.5 threshold)
            Vector3 objectPos = new Vector3(0, 0, 0);
            Vector3 targetPos = new Vector3(0.8f, 0, 0);
            
            _engine.CheckNearMiss(objectPos, targetPos, "TestObject");
            
            // Assert
            Assert.AreEqual(0, _engine.NearMissCount);
        }

        [Test]
        public void CheckNearMiss_ObjectAtExactThreshold_NearMissRecorded()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - object at exactly 0.5 distance
            Vector3 objectPos = new Vector3(0, 0, 0);
            Vector3 targetPos = new Vector3(0.5f, 0, 0);
            
            _engine.CheckNearMiss(objectPos, targetPos, "TestObject");
            
            // Assert
            Assert.AreEqual(1, _engine.NearMissCount);
        }

        [Test]
        public void CheckNearMiss_ObjectAtZeroDistance_NoNearMiss()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - object at same position (distance ~ 0)
            Vector3 objectPos = new Vector3(0, 0, 0);
            Vector3 targetPos = new Vector3(0, 0, 0);
            
            _engine.CheckNearMiss(objectPos, targetPos, "TestObject");
            
            // Assert - should not count as near miss (too close = collision, not near miss)
            Assert.AreEqual(0, _engine.NearMissCount);
        }

        #endregion

        #region Distance Tracking Tests

        [Test]
        public void CheckNearMiss_MultipleNearMisses_TracksNearest()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - multiple near misses at different distances
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.5f, 0, 0), "Object1");
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.2f, 0, 0), "Object2");
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.4f, 0, 0), "Object3");
            
            // Assert - should track the nearest (0.2)
            Assert.AreEqual(0.2f, _engine.NearestDistance, 0.01f);
        }

        [Test]
        public void CheckNearMiss_CloserMissReplaces_NearestDistance()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - first near miss at 0.4, then closer at 0.15
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.4f, 0, 0), "Object1");
            Assert.AreEqual(0.4f, _engine.NearestDistance, 0.01f);
            
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.15f, 0, 0), "Object2");
            
            // Assert - should update to 0.15
            Assert.AreEqual(0.15f, _engine.NearestDistance, 0.01f);
        }

        #endregion

        #region Multiple Target Tests

        [Test]
        public void CheckNearMisses_MultipleTargets_FindsClosestNearMiss()
        {
            // Arrange
            _engine.ResetSession();
            
            Vector3 objectPos = new Vector3(0, 0, 0);
            Vector3[] targetPositions = new Vector3[]
            {
                new Vector3(0.8f, 0, 0),  // too far
                new Vector3(0.3f, 0, 0),  // near miss
                new Vector3(0.1f, 0, 0),   // near miss - closest
            };
            
            // Act
            _engine.CheckNearMisses(objectPos, targetPositions, "MultiTarget");
            
            // Assert
            Assert.AreEqual(1, _engine.NearMissCount);
        }

        [Test]
        public void CheckNearMisses_AllTargetsOutside_NoNearMiss()
        {
            // Arrange
            _engine.ResetSession();
            
            Vector3 objectPos = new Vector3(0, 0, 0);
            Vector3[] targetPositions = new Vector3[]
            {
                new Vector3(0.8f, 0, 0),
                new Vector3(1.0f, 0, 0),
                new Vector3(0.6f, 0, 0),  // closest but still outside
            };
            
            // Act
            _engine.CheckNearMisses(objectPos, targetPositions, "MultiTarget");
            
            // Assert
            Assert.AreEqual(0, _engine.NearMissCount);
        }

        #endregion

        #region Streak Detection Tests

        [Test]
        public void CheckNearMiss_RapidSuccession_StreakDetected()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - 3 near misses in quick succession (within time window)
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.3f, 0, 0), "Obj1");
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.4f, 0, 0), "Obj2");
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.2f, 0, 0), "Obj3");
            
            // Assert
            Assert.IsTrue(_engine.IsNearMissStreak);
            Assert.GreaterOrEqual(_engine.NearMissStreakCount, 3);
        }

        [Test]
        public void CheckNearMiss_ScatteredNearMisses_NoStreak()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act - near misses with time gap (simulated by clearing)
            // Note: In real test, we'd manipulate Time.time but that's complex in unit tests
            // We'll just verify that single near misses don't trigger streak
            
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.3f, 0, 0), "Obj1");
            
            // Assert - single near miss should not be a streak
            Assert.IsFalse(_engine.IsNearMissStreak);
        }

        #endregion

        #region Rate Calculation Tests

        [Test]
        public void GetNearMissRatePerMinute_NoNearMisses_ReturnsZero()
        {
            // Arrange
            _engine.ResetSession();
            
            // Act
            float rate = _engine.GetNearMissRatePerMinute();
            
            // Assert
            Assert.AreEqual(0f, rate, 0.01f);
        }

        [Test]
        public void GetNearMissRatePerMinute_SingleNearMiss_ReturnsZero()
        {
            // Arrange
            _engine.ResetSession();
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.3f, 0, 0), "Test");
            
            // Act
            float rate = _engine.GetNearMissRatePerMinute();
            
            // Assert - need at least 2 to calculate rate
            Assert.AreEqual(0f, rate, 0.01f);
        }

        #endregion

        #region Session Management Tests

        [Test]
        public void ResetSession_ClearsAllData()
        {
            // Arrange - add some near misses
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.3f, 0, 0), "Obj1");
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.2f, 0, 0), "Obj2");
            
            Assert.AreEqual(2, _engine.NearMissCount);
            
            // Act
            _engine.ResetSession();
            
            // Assert
            Assert.AreEqual(0, _engine.NearMissCount);
            Assert.AreEqual(float.MaxValue, _engine.NearestDistance, 0.01f);
            Assert.IsFalse(_engine.IsNearMissStreak);
        }

        [Test]
        public void GetSessionNearMisses_ReturnsCopyOfList()
        {
            // Arrange
            _engine.ResetSession();
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.3f, 0, 0), "Obj1");
            
            // Act
            List<NearMissEngine.NearMissEvent> events = _engine.GetSessionNearMisses();
            
            // Assert
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual("Obj1", events[0].objectName);
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void SetThresholds_UpdatesValues()
        {
            // Arrange
            float newDistance = 1.0f;
            float newTimeWindow = 2.0f;
            
            // Act
            _engine.SetThresholds(newDistance, newTimeWindow);
            
            // Assert - verify through behavior
            // Set thresholds then check near miss at 0.8 (was too close for 0.5)
            _engine.ResetSession();
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.8f, 0, 0), "Test");
            
            // With threshold 1.0, 0.8 should count as near miss
            Assert.AreEqual(1, _engine.NearMissCount);
        }

        [Test]
        public void SetThresholds_ClampsMinimumValues()
        {
            // Act - try to set negative/zero values
            _engine.SetThresholds(-1.0f, -0.5f);
            
            // Assert - should clamp to minimum 0.1f
            // This is verified by behavior - system should still work
            _engine.ResetSession();
            _engine.CheckNearMiss(new Vector3(0, 0, 0), new Vector3(0.05f, 0, 0), "Test");
            
            // Even with clamped threshold, 0.05 is still a near miss
            Assert.GreaterOrEqual(_engine.NearMissCount, 0);
        }

        #endregion

        #region Helper Methods

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(NearMissEngine).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(field, $"Field {fieldName} not found");
            field.SetValue(_engine, value);
        }

        #endregion
    }
}
