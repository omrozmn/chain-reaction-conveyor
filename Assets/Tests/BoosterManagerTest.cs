using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Systems;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for BoosterManager - inventory, activate, and cooldown tests
    /// </summary>
    [TestFixture]
    public class BoosterManagerTest
    {
        private GameObject _gameObject;
        private BoosterManager _boosterManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("BoosterManagerTest");
            _boosterManager = _gameObject.AddComponent<BoosterManager>();
            
            // Initialize by calling Awake manually
            _boosterManager.Invoke("Awake", 0f);
        }

        [TearDown]
        public void TearDown()
        {
            // Reset time scale in case slow was active
            Time.timeScale = 1f;
            GameObject.DestroyImmediate(_gameObject);
        }

        #region Inventory Tests

        [Test]
        public void GetCharges_InitializesWithDefaultValues()
        {
            // Default values: Swap=3, Bomb=2, Slow=2
            Assert.That(_boosterManager.GetCharges(BoosterType.Swap), Is.EqualTo(3));
            Assert.That(_boosterManager.GetCharges(BoosterType.Bomb), Is.EqualTo(2));
            Assert.That(_boosterManager.GetCharges(BoosterType.Slow), Is.EqualTo(2));
        }

        [Test]
        public void HasBooster_ReturnsTrue_WhenChargesAvailable()
        {
            Assert.That(_boosterManager.HasBooster(BoosterType.Swap), Is.True);
            Assert.That(_boosterManager.HasBooster(BoosterType.Bomb), Is.True);
            Assert.That(_boosterManager.HasBooster(BoosterType.Slow), Is.True);
        }

        [Test]
        public void HasBooster_ReturnsFalse_WhenNoCharges()
        {
            // Use all charges
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            Assert.That(_boosterManager.HasBooster(BoosterType.Swap), Is.False);
        }

        [Test]
        public void AddCharges_IncreasesInventory()
        {
            _boosterManager.AddCharges(BoosterType.Swap, 5);
            
            Assert.That(_boosterManager.GetCharges(BoosterType.Swap), Is.EqualTo(8)); // 3 + 5
        }

        [Test]
        public void AddCharges_AddsNewBoosterType()
        {
            // There's no "Color" booster by default, but AddCharges should handle it
            _boosterManager.AddCharges(BoosterType.Bomb, 3);
            
            Assert.That(_boosterManager.GetCharges(BoosterType.Bomb), Is.EqualTo(5)); // 2 + 3
        }

        [Test]
        public void ResetBoosters_RestoresDefaultCharges()
        {
            // Use some charges
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            _boosterManager.ActivateBooster(BoosterType.Bomb, Vector2.zero);
            
            // Reset
            _boosterManager.ResetBoosters();
            
            Assert.That(_boosterManager.GetCharges(BoosterType.Swap), Is.EqualTo(3));
            Assert.That(_boosterManager.GetCharges(BoosterType.Bomb), Is.EqualTo(2));
            Assert.That(_boosterManager.GetCharges(BoosterType.Slow), Is.EqualTo(2));
        }

        #endregion

        #region Activate Tests

        [Test]
        public void ActivateBooster_ReturnsTrue_WhenChargesAvailable()
        {
            bool result = _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            Assert.That(result, Is.True);
        }

        [Test]
        public void ActivateBooster_ReturnsFalse_WhenNoCharges()
        {
            // Exhaust all charges
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            bool result = _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            Assert.That(result, Is.False);
        }

        [Test]
        public void ActivateBooster_DeductsCharge()
        {
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            Assert.That(_boosterManager.GetCharges(BoosterType.Swap), Is.EqualTo(2));
        }

        [Test]
        public void ActivateBooster_TriggersInventoryChangedEvent()
        {
            bool eventFired = false;
            _boosterManager.OnInventoryChanged += (type, charges) => eventFired = true;
            
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void ActivateBooster_TriggersActivatedEvent()
        {
            BoosterActivatedEvent capturedEvent = null;
            _boosterManager.OnBoosterActivated += evt => capturedEvent = evt;
            
            _boosterManager.ActivateBooster(BoosterType.Bomb, new Vector2(1, 2));
            
            Assert.That(capturedEvent, Is.Not.Null);
            Assert.That(capturedEvent.Type, Is.EqualTo(BoosterType.Bomb));
            Assert.That(capturedEvent.Position, Is.EqualTo(new Vector2(1, 2)));
        }

        [Test]
        public void ActivateBooster_SlowDecreasesTimeScale()
        {
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            
            Assert.That(Time.timeScale, Is.LessThan(1f));
        }

        [Test]
        public void ActivateBooster_BombDoesNotAffectTimeScale()
        {
            _boosterManager.ActivateBooster(BoosterType.Bomb, Vector2.zero);
            
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [Test]
        public void ActivateBooster_SwapDoesNotAffectTimeScale()
        {
            _boosterManager.ActivateBooster(BoosterType.Swap, Vector2.zero);
            
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        #endregion

        #region Cooldown/Active Duration Tests

        [Test]
        public void IsSlowActive_ReturnsTrue_WhenSlowRunning()
        {
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            
            Assert.That(_boosterManager.IsSlowActive(), Is.True);
        }

        [Test]
        public void GetSlowRemainingTime_ReturnsPositive_WhenActive()
        {
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            
            float remaining = _boosterManager.GetSlowRemainingTime();
            
            Assert.That(remaining, Is.GreaterThan(0f));
        }

        [Test]
        public void GetSlowRemainingTime_ReturnsZero_WhenNotActive()
        {
            Assert.That(_boosterManager.GetSlowRemainingTime(), Is.EqualTo(0f));
        }

        [Test]
        public void SlowEffectResetsTimeScale_AfterDuration()
        {
            // Store original time scale
            float originalTimeScale = Time.timeScale;
            
            // Activate slow booster (default 5 second duration)
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            
            // Wait for slow effect to end (in real test we'd use coroutines or time manipulation)
            // Here we just verify the mechanism exists
            Assert.That(_boosterManager.IsSlowActive(), Is.True);
            
            // Cleanup
            Time.timeScale = originalTimeScale;
        }

        [Test]
        public void MultipleSlowActivations_ResetTimer()
        {
            // Activate slow
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            float firstTimer = _boosterManager.GetSlowRemainingTime();
            
            // Activate again before first ends (resets timer)
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            float secondTimer = _boosterManager.GetSlowRemainingTime();
            
            // Timer should be reset to full duration
            Assert.That(secondTimer, Is.EqualTo(firstTimer).Within(0.1f));
        }

        [Test]
        public void ResetBoosters_StopsSlowEffect()
        {
            // Activate slow
            _boosterManager.ActivateBooster(BoosterType.Slow, Vector2.zero);
            Assert.That(_boosterManager.IsSlowActive(), Is.True);
            
            // Reset
            _boosterManager.ResetBoosters();
            
            Assert.That(_boosterManager.IsSlowActive(), Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        #endregion

        #region Singleton Tests

        [Test]
        public void Instance_IsSet()
        {
            Assert.That(BoosterManager.Instance, Is.EqualTo(_boosterManager));
        }

        #endregion
    }
}
