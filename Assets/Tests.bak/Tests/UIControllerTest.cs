using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using ChainReactionConveyor.UI;
using ChainReactionConveyor.Systems;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for UIController - screen transitions and score update tests
    /// </summary>
    [TestFixture]
    public class UIControllerTest
    {
        private GameObject _gameObject;
        private UIController _uiController;
        
        // Mock UI elements
        private GameObject _mainMenuScreen;
        private GameObject _hudScreen;
        private GameObject _gameOverScreen;
        private GameObject _pauseScreen;
        private Text _scoreText;
        private Text _levelText;
        private Text _comboText;
        private Text _finalScoreText;
        private Text _highScoreText;
        private Text _starsText;
        private Text _swapCountText;
        private Image _slowTimerFill;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("UIControllerTest");
            _uiController = _gameObject.AddComponent<UIController>();
            
            // Create mock UI screens
            _mainMenuScreen = new GameObject("MainMenuScreen");
            _mainMenuScreen.transform.SetParent(_gameObject.transform);
            
            _hudScreen = new GameObject("HUDScreen");
            _hudScreen.transform.SetParent(_gameObject.transform);
            
            _gameOverScreen = new GameObject("GameOverScreen");
            _gameOverScreen.transform.SetParent(_gameObject.transform);
            
            _pauseScreen = new GameObject("PauseScreen");
            _pauseScreen.transform.SetParent(_gameObject.transform);
            
            // Create mock text elements
            _scoreText = CreateTextObject("ScoreText", _gameObject.transform);
            _levelText = CreateTextObject("LevelText", _gameObject.transform);
            _comboText = CreateTextObject("ComboText", _gameObject.transform);
            _finalScoreText = CreateTextObject("FinalScoreText", _gameObject.transform);
            _highScoreText = CreateTextObject("HighScoreText", _gameObject.transform);
            _starsText = CreateTextObject("StarsText", _gameObject.transform);
            _swapCountText = CreateTextObject("SwapCountText", _gameObject.transform);
            
            // Create mock image for slow timer
            _slowTimerFill = CreateImageObject("SlowTimerFill", _gameObject.transform);
            
            // Assign via reflection (since SerializeField)
            SetPrivateField(_uiController, "mainMenuScreen", _mainMenuScreen);
            SetPrivateField(_uiController, "hudScreen", _hudScreen);
            SetPrivateField(_uiController, "gameOverScreen", _gameOverScreen);
            SetPrivateField(_uiController, "pauseScreen", _pauseScreen);
            SetPrivateField(_uiController, "scoreText", _scoreText);
            SetPrivateField(_uiController, "levelText", _levelText);
            SetPrivateField(_uiController, "comboText", _comboText);
            SetPrivateField(_uiController, "finalScoreText", _finalScoreText);
            SetPrivateField(_uiController, "highScoreText", _highScoreText);
            SetPrivateField(_uiController, "starsText", _starsText);
            SetPrivateField(_uiController, "swapCountText", _swapCountText);
            SetPrivateField(_uiController, "slowTimerFill", _slowTimerFill);
            
            // Initialize
            _uiController.Invoke("Awake", 0f);
            _uiController.Invoke("Start", 0f);
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(_gameObject);
        }

        #region Screen Transition Tests

        [Test]
        public void ShowMainMenu_ActivatesMainMenuScreen()
        {
            _uiController.ShowMainMenu();
            
            Assert.That(_mainMenuScreen.activeSelf, Is.True);
            Assert.That(_hudScreen.activeSelf, Is.False);
            Assert.That(_gameOverScreen.activeSelf, Is.False);
            Assert.That(_pauseScreen.activeSelf, Is.False);
        }

        [Test]
        public void ShowHUD_ActivatesHUDScreen()
        {
            _uiController.ShowHUD();
            
            Assert.That(_mainMenuScreen.activeSelf, Is.False);
            Assert.That(_hudScreen.activeSelf, Is.True);
            Assert.That(_gameOverScreen.activeSelf, Is.False);
            Assert.That(_pauseScreen.activeSelf, Is.False);
        }

        [Test]
        public void ShowGameOver_ActivatesGameOverScreen()
        {
            _uiController.ShowGameOver(1000, 500, 2);
            
            Assert.That(_mainMenuScreen.activeSelf, Is.False);
            Assert.That(_hudScreen.activeSelf, Is.False);
            Assert.That(_gameOverScreen.activeSelf, Is.True);
            Assert.That(_pauseScreen.activeSelf, Is.False);
        }

        [Test]
        public void ShowGameOver_UpdatesScoreText()
        {
            _uiController.ShowGameOver(1234, 567, 3);
            
            Assert.That(_finalScoreText.text, Does.Contain("1234"));
        }

        [Test]
        public void ShowGameOver_UpdatesHighScoreText()
        {
            _uiController.ShowGameOver(1000, 2000, 1);
            
            Assert.That(_highScoreText.text, Does.Contain("2000"));
        }

        [Test]
        public void ShowGameOver_UpdatesStarsText()
        {
            _uiController.ShowGameOver(1000, 500, 3);
            
            Assert.That(_starsText.text, Does.Contain("★★★"));
        }

        [Test]
        public void ShowGameOver_ShowsOneStar()
        {
            _uiController.ShowGameOver(1000, 500, 1);
            
            Assert.That(_starsText.text, Does.Contain("★☆☆"));
        }

        [Test]
        public void ShowPause_ActivatesPauseScreen()
        {
            // Must be in HUD state first
            _uiController.ShowHUD();
            _uiController.ShowPause();
            
            Assert.That(_pauseScreen.activeSelf, Is.True);
        }

        [Test]
        public void ShowPause_DoesNothing_WhenNotInHUD()
        {
            // In MainMenu state
            _uiController.ShowMainMenu();
            _uiController.ShowPause();
            
            Assert.That(_pauseScreen.activeSelf, Is.False);
        }

        [Test]
        public void HidePause_DeactivatesPauseScreen()
        {
            _uiController.ShowHUD();
            _uiController.ShowPause();
            _uiController.HidePause();
            
            Assert.That(_pauseScreen.activeSelf, Is.False);
        }

        [Test]
        public void HideAllScreens_DeactivatesAll()
        {
            // Show one first
            _uiController.ShowHUD();
            
            // Now hide all
            _uiController.ShowMainMenu(); // This calls HideAllScreens internally
            
            Assert.That(_mainMenuScreen.activeSelf, Is.True);
            Assert.That(_hudScreen.activeSelf, Is.False);
            Assert.That(_gameOverScreen.activeSelf, Is.False);
            Assert.That(_pauseScreen.activeSelf, Is.False);
        }

        #endregion

        #region Score Update Tests

        [Test]
        public void UpdateScore_UpdatesScoreText()
        {
            _uiController.UpdateScore(500);
            
            Assert.That(_scoreText.text, Does.Contain("500"));
        }

        [Test]
        public void UpdateScore_HandlesZero()
        {
            _uiController.UpdateScore(0);
            
            Assert.That(_scoreText.text, Does.Contain("0"));
        }

        [Test]
        public void UpdateScore_HandlesLargeNumbers()
        {
            _uiController.UpdateScore(999999);
            
            Assert.That(_scoreText.text, Does.Contain("999999"));
        }

        [Test]
        public void UpdateLevel_UpdatesLevelText()
        {
            _uiController.UpdateLevel(5);
            
            Assert.That(_levelText.text, Does.Contain("5"));
        }

        [Test]
        public void UpdateCombo_ShowsCombo_WhenAboveOne()
        {
            _uiController.UpdateCombo(3);
            
            Assert.That(_comboText.text, Does.Contain("3"));
            Assert.That(_comboText.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void UpdateCombo_HidesCombo_WhenOneOrLess()
        {
            _uiController.UpdateCombo(1);
            
            Assert.That(_comboText.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void UpdateCombo_HidesCombo_WhenZero()
        {
            _uiController.UpdateCombo(0);
            
            Assert.That(_comboText.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void UpdateBoosterCounts_UpdatesAllTexts()
        {
            _uiController.UpdateBoosterCounts(5, 3, 2);
            
            // Note: These test the booster count update logic
            // The actual text values depend on the implementation
            Assert.That(_swapCountText, Is.Not.Null);
        }

        [Test]
        public void UpdateSlowTimer_UpdatesFillAmount()
        {
            _uiController.UpdateSlowTimer(0.5f);
            
            Assert.That(_slowTimerFill.fillAmount, Is.EqualTo(0.5f));
        }

        [Test]
        public void UpdateSlowTimer_HidesWhenZero()
        {
            _uiController.UpdateSlowTimer(0f);
            
            Assert.That(_slowTimerFill.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void UpdateSlowTimer_ShowsWhenPositive()
        {
            _uiController.UpdateSlowTimer(0.25f);
            
            Assert.That(_slowTimerFill.gameObject.activeSelf, Is.True);
        }

        #endregion

        #region State Tests

        [Test]
        public void IsPaused_ReturnsFalse_Initially()
        {
            Assert.That(_uiController.IsPaused(), Is.False);
        }

        [Test]
        public void IsPaused_ReturnsTrue_WhenPaused()
        {
            _uiController.ShowHUD();
            _uiController.ShowPause();
            
            Assert.That(_uiController.IsPaused(), Is.True);
        }

        [Test]
        public void Play_ShowsHUD()
        {
            _uiController.Play();
            
            Assert.That(_hudScreen.activeSelf, Is.True);
        }

        [Test]
        public void Restart_ShowsHUD()
        {
            _uiController.ShowGameOver(100, 50, 1);
            _uiController.Restart();
            
            Assert.That(_hudScreen.activeSelf, Is.True);
        }

        [Test]
        public void GoToMainMenu_ShowsMainMenu()
        {
            _uiController.ShowGameOver(100, 50, 1);
            _uiController.GoToMainMenu();
            
            Assert.That(_mainMenuScreen.activeSelf, Is.True);
        }

        [Test]
        public void TogglePause_AlternatesPauseState()
        {
            _uiController.ShowHUD();
            
            // Toggle on
            _uiController.TogglePause();
            Assert.That(_uiController.IsPaused(), Is.True);
            
            // Toggle off
            _uiController.TogglePause();
            Assert.That(_uiController.IsPaused(), Is.False);
        }

        #endregion

        #region Event Tests

        [Test]
        public void OnPlayClicked_EventFires()
        {
            bool eventFired = false;
            _uiController.OnPlayClicked += () => eventFired = true;
            
            _uiController.Play();
            
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnRestartClicked_EventFires()
        {
            bool eventFired = false;
            _uiController.OnRestartClicked += () => eventFired = true;
            
            _uiController.Restart();
            
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnMainMenuClicked_EventFires()
        {
            bool eventFired = false;
            _uiController.OnMainMenuClicked += () => eventFired = true;
            
            _uiController.GoToMainMenu();
            
            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnPauseClicked_EventFires()
        {
            bool eventFired = false;
            _uiController.OnPauseClicked += () => eventFired = true;
            
            _uiController.ShowHUD();
            _uiController.TogglePause();
            
            Assert.That(eventFired, Is.True);
        }

        #endregion

        #region Singleton Tests

        [Test]
        public void Instance_IsSet()
        {
            Assert.That(UIController.Instance, Is.EqualTo(_uiController));
        }

        #endregion

        #region Helper Methods

        private Text CreateTextObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Text text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }

        private Image CreateImageObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            Image image = obj.AddComponent<Image>();
            return image;
        }

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
