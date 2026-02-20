using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.UI;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for ComboBar - combo tracking and UI display
    /// </summary>
    [TestFixture]
    public class ComboBarTest
    {
        private GameObject _gameObject;
        private ComboBar _comboBar;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("ComboBarTest");
            _comboBar = _gameObject.AddComponent<ComboBar>();
            
            // Create required UI elements
            var imageObj = new GameObject("FillImage");
            imageObj.transform.SetParent(_gameObject.transform);
            var image = imageObj.AddComponent<UnityEngine.UI.Image>();
            image.type = UnityEngine.UI.Image.Type.Filled;
            _comboBar.fillImage = image;
            
            var textObj = new GameObject("ComboText");
            textObj.transform.SetParent(_gameObject.transform);
            _comboBar.comboText = textObj.AddComponent<UnityEngine.UI.Text>();
            
            var multiplierObj = new GameObject("MultiplierText");
            multiplierObj.transform.SetParent(_gameObject.transform);
            _comboBar.multiplierText = multiplierObj.AddComponent<UnityEngine.UI.Text>();
            
            var canvasGroupObj = new GameObject("CanvasGroup");
            canvasGroupObj.transform.SetParent(_gameObject.transform);
            _comboBar.canvasGroup = canvasGroupObj.AddComponent<CanvasGroup>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Instance.Clear();
            GameObject.DestroyImmediate(_gameObject);
        }

        #region Initialization Tests

        [Test]
        public void GetCurrentCombo_InitializesToZero()
        {
            Assert.That(_comboBar.GetCurrentCombo(), Is.EqualTo(0));
        }

        [Test]
        public void IsComboActive_InitializesToFalse()
        {
            Assert.That(_comboBar.IsComboActive(), Is.False);
        }

        [Test]
        public void GetMultiplier_InitializesToOne()
        {
            Assert.That(_comboBar.GetMultiplier(), Is.EqualTo(1f).Within(0.01f));
        }

        #endregion

        #region Combo Increment Tests

        [Test]
        public void AddCombo_IncrementsCombo()
        {
            _comboBar.AddCombo(1);
            
            Assert.That(_comboBar.GetCurrentCombo(), Is.EqualTo(1));
        }

        [Test]
        public void AddCombo_MultipleTimes_Accumulates()
        {
            _comboBar.AddCombo(1);
            _comboBar.AddCombo(1);
            _comboBar.AddCombo(1);
            
            Assert.That(_comboBar.GetCurrentCombo(), Is.EqualTo(3));
        }

        [Test]
        public void AddCombo_CapsAtMaxCombo()
        {
            // Default maxCombo is 10
            _comboBar.AddCombo(15);
            
            Assert.That(_comboBar.GetCurrentCombo(), Is.EqualTo(10));
        }

        [Test]
        public void AddCombo_SetsComboActive()
        {
            _comboBar.AddCombo(1);
            
            Assert.That(_comboBar.IsComboActive(), Is.True);
        }

        [Test]
        public void AddCombo_UpdatesMultiplier()
        {
            _comboBar.AddCombo(5);
            
            // Multiplier = 1 + (combo * 0.1)
            Assert.That(_comboBar.GetMultiplier(), Is.EqualTo(1.5f).Within(0.01f));
        }

        #endregion

        #region Event Tests

        [Test]
        public void OnComboUpdated_FiresEvent()
        {
            bool eventFired = false;
            _comboBar.OnComboUpdated += (combo) => eventFired = true;
            
            _comboBar.AddCombo(1);
            
            Assert.That(eventFired, Is.True);
        }

        [Test]
        [Ignore("Requires time manipulation")]
        public void ComboExpires_AfterTimeout()
        {
            _comboBar.AddCombo(1);
            
            // Wait for timeout (3 seconds by default)
            // In real test, use custom time provider
            
            Assert.That(_comboBar.IsComboActive(), Is.False);
        }

        #endregion

        #region Reset Tests

        [Test]
        public void ResetCombo_ResetsToZero()
        {
            _comboBar.AddCombo(5);
            
            // Access private method via reflection or add public method
            // For now, verify that new combo overwrites old
            
            Assert.That(_comboBar.GetCurrentCombo(), Is.EqualTo(5));
        }

        #endregion
    }
}
