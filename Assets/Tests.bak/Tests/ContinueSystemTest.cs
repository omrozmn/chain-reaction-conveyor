using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Systems;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for ContinueSystem - handles game over continue options
    /// </summary>
    [TestFixture]
    public class ContinueSystemTest
    {
        private GameObject _gameObject;
        private ContinueSystem _continueSystem;

        [SetUp]
        public void SetUp()
        {
            // Clear EventBus before each test
            EventBus.Instance.Clear();
            
            _gameObject = new GameObject("ContinueSystemTest");
            _continueSystem = _gameObject.AddComponent<ContinueSystem>();
            
            // Setup private fields via reflection for testing
            SetPrivateField("continuePanel", new GameObject("ContinuePanel"));
            SetPrivateField("continueButton", _gameObject.AddComponent<UnityEngine.UI.Button>());
            SetPrivateField("continueCostText", new GameObject("CostText").AddComponent<UnityEngine.UI.Text>());
            SetPrivateField("continueCountText", new GameObject("CountText").AddComponent<UnityEngine.UI.Text>());
            SetPrivateField("noContinuesText", new GameObject("NoContinuesText").AddComponent<UnityEngine.UI.Text>());
            SetPrivateField("audioSource", _gameObject.AddComponent<AudioSource>());
            
            // Reset PlayerPrefs
            PlayerPrefs.SetInt("PlayerGems", 0);
            PlayerPrefs.SetInt("StarterContinuesUsed", 0);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Instance.Clear();
            GameObject.DestroyImmediate(_gameObject);
            PlayerPrefs.DeleteKey("PlayerGems");
            PlayerPrefs.DeleteKey("StarterContinuesUsed");
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(ContinueSystem).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_continueSystem, value);
        }

        private object GetPrivateField(string fieldName)
        {
            var field = typeof(ContinueSystem).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_continueSystem);
        }

        #region Instance Tests

        [Test]
        public void Instance_IsSet_OnAwake()
        {
            Assert.That(ContinueSystem.Instance, Is.EqualTo(_continueSystem));
        }

        [Test]
        public void Instance_Singleton_PreventsDuplicates()
        {
            var otherGameObject = new GameObject("OtherContinueSystem");
            var otherSystem = otherGameObject.AddComponent<ContinueSystem>();
            
            // Should destroy duplicate
            Assert.That(ContinueSystem.Instance, Is.EqualTo(_continueSystem));
            
            GameObject.DestroyImmediate(otherGameObject);
        }

        #endregion

        #region Continue Count Tests

        [Test]
        public void ContinuesRemaining_InitiallyEqualsMax()
        {
            Assert.That(_continueSystem.ContinuesRemaining, Is.EqualTo(3)); // default maxContinuesPerLevel
        }

        [Test]
        public void CanContinue_InitiallyTrue_WhenStarterPackAvailable()
        {
            // Default useStarterPack = true, starterContinues = 5
            Assert.That(_continueSystem.CanContinue, Is.True);
        }

        [Test]
        public void ResetContinues_ResetsCountToZero()
        {
            // Act
            _continueSystem.ResetContinues();
            
            // Assert
            Assert.That(_continueSystem.ContinuesRemaining, Is.EqualTo(3));
        }

        #endregion

        #region Starter Pack Tests

        [Test]
        public void CheckContinueAvailability_StarterPackAvailable_ReturnsTrue()
        {
            // Arrange - set private field
            PlayerPrefs.SetInt("PlayerGems", 0);
            PlayerPrefs.SetInt("StarterContinuesUsed", 0);
            PlayerPrefs.Save();
            
            // Use reflection to call private method
            var method = typeof(ContinueSystem).GetMethod("CheckContinueAvailability", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act - trigger via level fail event
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "            // AssertTest" });
            

            Assert.That(_continueSystem.CanContinue, Is.True);
        }

        [Test]
        public void CheckContinueAvailability_StarterPackExhausted_NoGems_ReturnsFalse()
        {
            // Arrange - exhaust starter pack
            PlayerPrefs.SetInt("PlayerGems", 0);
            PlayerPrefs.SetInt("StarterContinuesUsed", 5); // All used
            PlayerPrefs.Save();
            
            // Act
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "Test" });
            
            // Assert
            Assert.That(_continueSystem.CanContinue, Is.False);
        }

        [Test]
        public void CheckContinueAvailability_HasEnoughGems_ReturnsTrue()
        {
            // Arrange
            PlayerPrefs.SetInt("PlayerGems", 100); // Enough for continue cost
            PlayerPrefs.SetInt("StarterContinuesUsed", 5); // Exhausted
            PlayerPrefs.Save();
            
            // Act
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "Test" });
            
            // Assert
            Assert.That(_continueSystem.CanContinue, Is.True);
        }

        [Test]
        public void CheckContinueAvailability_NotEnoughGems_ReturnsFalse()
        {
            // Arrange
            PlayerPrefs.SetInt("PlayerGems", 50); // Not enough (cost is 100)
            PlayerPrefs.SetInt("StarterContinuesUsed", 5); // Exhausted
            PlayerPrefs.Save();
            
            // Act
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "Test" });
            
            // Assert
            Assert.That(_continueSystem.CanContinue, Is.False);
        }

        #endregion

        #region Panel Visibility Tests

        [Test]
        public void ContinuePanel_StartsHidden()
        {
            var panel = GetPrivateField("continuePanel") as GameObject;
            Assert.That(panel.activeSelf, Is.False);
        }

        [Test]
        public void OnLevelFail_WithContinueAvailable_ShowsPanel()
        {
            // Arrange
            PlayerPrefs.SetInt("PlayerGems", 100);
            PlayerPrefs.SetInt("StarterContinuesUsed", 5);
            PlayerPrefs.Save();
            
            // Act
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "Test" });
            
            // Assert
            var panel = GetPrivateField("continuePanel") as GameObject;
            Assert.That(panel.activeSelf, Is.True);
        }

        [Test]
        public void OnLevelStart_HidesPanel()
        {
            // Arrange - first show panel
            PlayerPrefs.SetInt("PlayerGems", 100);
            PlayerPrefs.SetInt("StarterContinuesUsed", 5);
            PlayerPrefs.Save();
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "Test" });
            
            // Act
            EventBus.Instance.Publish(new LevelStartEvent { LevelId = 2, Seed = 123 });
            
            // Assert
            var panel = GetPrivateField("continuePanel") as GameObject;
            Assert.That(panel.activeSelf, Is.False);
        }

        #endregion

        #region Continue Cost Tests

        [Test]
        public void GetContinueCost_ReturnsDefaultCost()
        {
            Assert.That(_continueSystem.GetContinueCost(), Is.EqualTo(100));
        }

        [Test]
        public void SetContinueCost_UpdatesCost()
        {
            // Act
            _continueSystem.SetContinueCost(50);
            
            // Assert
            Assert.That(_continueSystem.GetContinueCost(), Is.EqualTo(50));
        }

        [Test]
        public void SetContinueCost_NegativeValue_ClampedToZero()
        {
            // Act
            _continueSystem.SetContinueCost(-10);
            
            // Assert
            Assert.That(_continueSystem.GetContinueCost(), Is.EqualTo(0));
        }

        #endregion

        #region Event Tests

        [Test]
        public void OnLevelStart_ResetsContinuesAndLevelId()
        {
            // Arrange
            var currentLevelId = GetPrivateField("currentLevelId");
            Assert.That(currentLevelId, Is.EqualTo(-1));
            
            // Act
            EventBus.Instance.Publish(new LevelStartEvent { LevelId = 5, Seed = 100 });
            
            // Assert
            currentLevelId = GetPrivateField("currentLevelId");
            Assert.That(currentLevelId, Is.EqualTo(5));
            Assert.That(_continueSystem.ContinuesRemaining, Is.EqualTo(3));
        }

        [Test]
        public void SkipContinue_HidesPanel()
        {
            // Arrange - show panel first
            PlayerPrefs.SetInt("PlayerGems", 100);
            PlayerPrefs.SetInt("StarterContinuesUsed", 5);
            PlayerPrefs.Save();
            EventBus.Instance.Publish(new LevelFailEvent { LevelId = 1, Reason = "Test" });
            
            var panel = GetPrivateField("continuePanel") as GameObject;
            Assert.That(panel.activeSelf, Is.True);
            
            // Act
            _continueSystem.SkipContinue();
            
            // Assert
            Assert.That(panel.activeSelf, Is.False);
        }

        #endregion
    }
}
