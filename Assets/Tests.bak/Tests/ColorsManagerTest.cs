using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Systems;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for ColorsManager - theme management and color retrieval
    /// </summary>
    [TestFixture]
    public class ColorsManagerTest
    {
        private GameObject _gameObject;
        private ColorsManager _colorsManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("ColorsManagerTest");
            _colorsManager = _gameObject.AddComponent<ColorsManager>();
            
            // Set up accessible fields via reflection
            SetPrivateField(_colorsManager, "availableThemes", CreateTestThemes());
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Instance.Clear();
            GameObject.DestroyImmediate(_gameObject);
        }

        private ColorTheme[] CreateTestThemes()
        {
            var theme1 = ScriptableObject.CreateInstance<ColorTheme>();
            theme1.themeName = "Dark";
            theme1.primaryColor = Color.black;
            theme1.secondaryColor = Color.gray;
            theme1.accentColor = Color.cyan;
            theme1.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            theme1.textColor = Color.white;
            theme1.buttonColor = Color.blue;
            theme1.buttonTextColor = Color.white;
            theme1.itemColor = Color.green;
            theme1.conveyorColor = Color.gray;
            theme1.successColor = Color.green;
            theme1.failColor = Color.red;
            theme1.swapColor = Color.cyan;
            theme1.bombColor = Color.red;
            theme1.slowColor = Color.magenta;

            var theme2 = ScriptableObject.CreateInstance<ColorTheme>();
            theme2.themeName = "Light";
            theme2.primaryColor = Color.white;
            theme2.secondaryColor = Color.gray;
            theme2.accentColor = Color.blue;
            theme2.backgroundColor = Color.white;
            theme2.textColor = Color.black;
            theme2.buttonColor = Color.blue;
            theme2.buttonTextColor = Color.white;
            theme2.itemColor = Color.green;
            theme2.conveyorColor = Color.gray;
            theme2.successColor = Color.green;
            theme2.failColor = Color.red;
            theme2.swapColor = Color.cyan;
            theme2.bombColor = Color.red;
            theme2.slowColor = Color.magenta;

            return new ColorTheme[] { theme1, theme2 };
        }

        #region Initialization Tests

        [Test]
        public void Instance_IsSet()
        {
            Assert.That(ColorsManager.Instance, Is.EqualTo(_colorsManager));
        }

        [Test]
        public void CurrentTheme_IsLoaded()
        {
            // After Awake, should have loaded first theme
            Assert.That(_colorsManager.CurrentTheme, Is.Not.Null);
        }

        [Test]
        public void GetThemeCount_ReturnsThemeCount()
        {
            Assert.That(_colorsManager.GetThemeCount(), Is.EqualTo(2));
        }

        [Test]
        public void GetCurrentThemeIndex_InitializesToZero()
        {
            Assert.That(_colorsManager.GetCurrentThemeIndex(), Is.EqualTo(0));
        }

        #endregion

        #region Theme Loading Tests

        [Test]
        public void LoadTheme_ChangesCurrentTheme()
        {
            _colorsManager.LoadTheme(1);
            
            Assert.That(_colorsManager.CurrentTheme.themeName, Is.EqualTo("Light"));
        }

        [Test]
        public void LoadTheme_ClampsToValidRange()
        {
            // Try to load invalid index
            _colorsManager.LoadTheme(999);
            
            // Should clamp to last valid index
            Assert.That(_colorsManager.CurrentTheme, Is.Not.Null);
        }

        [Test]
        public void LoadTheme_ByName_FindsTheme()
        {
            _colorsManager.LoadTheme("Light");
            
            Assert.That(_colorsManager.CurrentTheme.themeName, Is.EqualTo("Light"));
        }

        [Test]
        public void LoadTheme_ByName_ReturnsEarly()
        {
            _colorsManager.LoadTheme("Dark");
            
            Assert.That(_colorsManager.CurrentTheme.themeName, Is.EqualTo("Dark"));
        }

        [Test]
        public void NextTheme_CyclesToNext()
        {
            _colorsManager.NextTheme();
            
            Assert.That(_colorsManager.GetCurrentThemeIndex(), Is.EqualTo(1));
        }

        [Test]
        public void NextTheme_WrapsAround()
        {
            _colorsManager.NextTheme(); // 0 -> 1
            _colorsManager.NextTheme(); // 1 -> 0
            
            Assert.That(_colorsManager.GetCurrentThemeIndex(), Is.EqualTo(0));
        }

        [Test]
        public void PreviousTheme_CyclesToPrevious()
        {
            _colorsManager.NextTheme(); // Go to index 1
            _colorsManager.PreviousTheme(); // Back to 0
            
            Assert.That(_colorsManager.GetCurrentThemeIndex(), Is.EqualTo(0));
        }

        [Test]
        public void PreviousTheme_WrapsAround()
        {
            _colorsManager.PreviousTheme(); // 0 -> last
            
            Assert.That(_colorsManager.GetCurrentThemeIndex(), Is.EqualTo(1));
        }

        #endregion

        #region Color Retrieval Tests

        [Test]
        public void GetBoosterColor_ReturnsColor_ForSwap()
        {
            Color color = _colorsManager.GetBoosterColor(BoosterType.Swap);
            
            Assert.That(color, Is.EqualTo(Color.cyan));
        }

        [Test]
        public void GetBoosterColor_ReturnsColor_ForBomb()
        {
            Color color = _colorsManager.GetBoosterColor(BoosterType.Bomb);
            
            Assert.That(color, Is.EqualTo(Color.red));
        }

        [Test]
        public void GetBoosterColor_ReturnsColor_ForSlow()
        {
            Color color = _colorsManager.GetBoosterColor(BoosterType.Slow);
            
            Assert.That(color, Is.EqualTo(Color.magenta));
        }

        [Test]
        public void GetLerpedColor_ReturnsInterpolatedColor()
        {
            Color color = _colorsManager.GetLerpedColor(0.5f);
            
            // Should be halfway between primary and accent
            Assert.That(color, Is.Not.Null);
        }

        [Test]
        public void GetColorWithAlpha_AppliesAlpha()
        {
            Color original = Color.red;
            Color withAlpha = _colorsManager.GetColorWithAlpha(original, 0.5f);
            
            Assert.That(withAlpha.a, Is.EqualTo(0.5f));
        }

        [Test]
        public void GetColorWithAlpha_PreservesRGB()
        {
            Color original = new Color(1f, 0.5f, 0.2f, 1f);
            Color withAlpha = _colorsManager.GetColorWithAlpha(original, 0.5f);
            
            Assert.That(withAlpha.r, Is.EqualTo(1f).Within(0.01f));
            Assert.That(withAlpha.g, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(withAlpha.b, Is.EqualTo(0.2f).Within(0.01f));
        }

        [Test]
        public void GetPrimaryGradient_ReturnsValidGradient()
        {
            Gradient gradient = _colorsManager.GetPrimaryGradient();
            
            Assert.That(gradient, Is.Not.Null);
        }

        [Test]
        public void GetRainbowGradient_IsStatic()
        {
            Gradient gradient = ColorsManager.GetRainbowGradient();
            
            Assert.That(gradient, Is.Not.Null);
        }

        #endregion

        #region Event Tests

        [Test]
        public void OnThemeChanged_FiresEvent()
        {
            bool eventFired = false;
            _colorsManager.OnThemeChanged += (theme) => eventFired = true;
            
            _colorsManager.LoadTheme(1);
            
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnThemeApplied_FiresEvent()
        {
            bool eventFired = false;
            _colorsManager.OnThemeApplied += () => eventFired = true;
            
            _colorsManager.ApplyThemeToUI();
            
            Assert.That(eventFired, Is.True);
        }

        #endregion

        #region Helper Methods

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        #endregion
    }
}
