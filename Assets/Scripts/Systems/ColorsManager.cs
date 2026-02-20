using UnityEngine;
using System;
using System.Collections.Generic;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Theme definitions for the game
    /// </summary>
    [CreateAssetMenu(fileName = "ColorTheme", menuName = "ChainReactionConveyor/ColorTheme")]
    public class ColorTheme : ScriptableObject
    {
        [Header("Theme Name")]
        public string themeName = "Default";
        
        [Header("Primary Colors")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.gray;
        public Color accentColor = Color.yellow;
        
        [Header("UI Colors")]
        public Color backgroundColor = Color.black;
        public Color textColor = Color.white;
        public Color buttonColor = Color.blue;
        public Color buttonTextColor = Color.white;
        
        [Header("Game Colors")]
        public Color itemColor = Color.green;
        public Color conveyorColor = Color.gray;
        public Color successColor = Color.green;
        public Color failColor = Color.red;
        
        [Header("Booster Colors")]
        public Color swapColor = new Color(0f, 0.8f, 1f); // Cyan
        public Color bombColor = new Color(1f, 0.3f, 0f); // Orange
        public Color slowColor = new Color(0.5f, 0f, 1f); // Purple
    }

    /// <summary>
    /// Event data for theme changes
    /// </summary>
    public class ThemeChangedEvent
    {
        public ColorTheme Theme { get; set; }
        public int ThemeIndex { get; set; }
    }

    /// <summary>
    /// Manages all color themes in the game
    /// Uses IComponent pattern for Unity integration
    /// </summary>
    public class ColorsManager : MonoBehaviour, IEventSubscriber
    {
        public static ColorsManager Instance { get; private set; }

        [Header("Themes")]
        [SerializeField] private ColorTheme[] availableThemes;
        [SerializeField] private int currentThemeIndex = 0;

        [Header("Settings")]
        [SerializeField] private bool applyOnStart = true;
        [SerializeField] private bool persistTheme = true;

        // Current theme
        public ColorTheme CurrentTheme { get; private set; }

        #region IEventSubscriber

        public void Subscribe()
        {
            EventBus.Instance.Subscribe<LevelStartEvent>(OnLevelStart);
            EventBus.Instance.Subscribe<LevelCompleteEvent>(OnLevelComplete);
        }

        public void Unsubscribe()
        {
            EventBus.Instance.Clear<LevelStartEvent>();
            EventBus.Instance.Clear<LevelCompleteEvent>();
        }

        #endregion

        // Events
        public event Action<ColorTheme> OnThemeChanged;
        public event Action OnThemeApplied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadTheme(currentThemeIndex);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyThemeToUI();
            }
        }

        private void OnLevelStart(LevelStartEvent evt)
        {
            // Optionally reset theme or apply on level start
            if (applyOnStart)
            {
                ApplyThemeToUI();
            }
        }

        private void OnLevelComplete(LevelCompleteEvent evt)
        {
            // Could apply success/fail theme colors here
        }

        /// <summary>
        /// Load a theme by index
        /// </summary>
        public void LoadTheme(int index)
        {
            if (availableThemes == null || availableThemes.Length == 0)
            {
                Debug.LogWarning("[ColorsManager] No themes available, using defaults");
                CurrentTheme = CreateDefaultTheme();
                return;
            }

            index = Mathf.Clamp(index, 0, availableThemes.Length - 1);
            CurrentTheme = availableThemes[index];
            currentThemeIndex = index;

            // Persist preference
            if (persistTheme)
            {
                PlayerPrefs.SetInt("SelectedTheme", index);
            }

            Debug.Log($"[ColorsManager] Loaded theme: {CurrentTheme.themeName}");
            
            // Fire local event
            OnThemeChanged?.Invoke(CurrentTheme);
            
            // Publish to EventBus
            EventBus.Instance.Publish(new ThemeChangedEvent
            {
                Theme = CurrentTheme,
                ThemeIndex = currentThemeIndex
            });
        }

        /// <summary>
        /// Load theme by name
        /// </summary>
        public void LoadTheme(string themeName)
        {
            if (availableThemes == null) return;

            for (int i = 0; i < availableThemes.Length; i++)
            {
                if (availableThemes[i].themeName == themeName)
                {
                    LoadTheme(i);
                    return;
                }
            }

            Debug.LogWarning($"[ColorsManager] Theme not found: {themeName}");
        }

        /// <summary>
        /// Load persisted theme
        /// </summary>
        public void LoadPersistedTheme()
        {
            if (persistTheme && PlayerPrefs.HasKey("SelectedTheme"))
            {
                LoadTheme(PlayerPrefs.GetInt("SelectedTheme"));
            }
        }

        /// <summary>
        /// Get next theme in list
        /// </summary>
        public void NextTheme()
        {
            if (availableThemes == null || availableThemes.Length <= 1) return;
            int nextIndex = (currentThemeIndex + 1) % availableThemes.Length;
            LoadTheme(nextIndex);
        }

        /// <summary>
        /// Get previous theme in list
        /// </summary>
        public void PreviousTheme()
        {
            if (availableThemes == null || availableThemes.Length <= 1) return;
            int prevIndex = (currentThemeIndex - 1 + availableThemes.Length) % availableThemes.Length;
            LoadTheme(prevIndex);
        }

        /// <summary>
        /// Apply theme to all UI elements
        /// </summary>
        public void ApplyThemeToUI()
        {
            if (CurrentTheme == null) return;

            // Apply to Canvas
            var canvases = FindObjectsByType<UnityEngine.Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                var image = canvas.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = CurrentTheme.backgroundColor;
                }
            }

            // Apply to all UI Images and Text tagged as Themeable
            var images = FindObjectsByType<UnityEngine.UI.Image>(FindObjectsSortMode.None);
            foreach (var image in images)
            {
                if (image.CompareTag("Themeable"))
                {
                    image.color = CurrentTheme.secondaryColor;
                }
            }

            var texts = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None);
            foreach (var text in texts)
            {
                if (text.CompareTag("Themeable"))
                {
                    text.color = CurrentTheme.textColor;
                }
            }

            Debug.Log("[ColorsManager] Theme applied to UI");
            OnThemeApplied?.Invoke();
        }

        /// <summary>
        /// Apply theme to materials
        /// </summary>
        public void ApplyThemeToMaterials()
        {
            if (CurrentTheme == null) return;

            // Apply conveyor color
            var conveyors = FindObjectsOfType<Mechanics.ConveyorMechanic>();
            foreach (var conveyor in conveyors)
            {
                var renderer = conveyor.GetComponent<UnityEngine.SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = CurrentTheme.conveyorColor;
                }
            }

            Debug.Log("[ColorsManager] Theme applied to materials");
        }

        /// <summary>
        /// Get color for booster type
        /// </summary>
        public Color GetBoosterColor(BoosterType type)
        {
            if (CurrentTheme == null) return Color.white;

            switch (type)
            {
                case BoosterType.Swap:
                    return CurrentTheme.swapColor;
                case BoosterType.Bomb:
                    return CurrentTheme.bombColor;
                case BoosterType.Slow:
                    return CurrentTheme.slowColor;
                default:
                    return CurrentTheme.accentColor;
            }
        }

        /// <summary>
        /// Get a lerped color between primary and accent
        /// </summary>
        public Color GetLerpedColor(float t)
        {
            if (CurrentTheme == null) return Color.Lerp(Color.white, Color.black, t);
            return Color.Lerp(CurrentTheme.primaryColor, CurrentTheme.accentColor, t);
        }

        /// <summary>
        /// Create default theme (fallback)
        /// </summary>
        private ColorTheme CreateDefaultTheme()
        {
            var theme = ScriptableObject.CreateInstance<ColorTheme>();
            theme.themeName = "Default";
            theme.primaryColor = Color.white;
            theme.secondaryColor = Color.gray;
            theme.accentColor = Color.yellow;
            theme.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            theme.textColor = Color.white;
            theme.buttonColor = Color.blue;
            theme.buttonTextColor = Color.white;
            return theme;
        }

        /// <summary>
        /// Get available theme count
        /// </summary>
        public int GetThemeCount() => availableThemes?.Length ?? 0;

        /// <summary>
        /// Get current theme index
        /// </summary>
        public int GetCurrentThemeIndex() => currentThemeIndex;

        /// <summary>
        /// Get color with alpha for UI elements
        /// </summary>
        public Color GetColorWithAlpha(Color color, float alpha)
        {
            Color c = color;
            c.a = alpha;
            return c;
        }

        /// <summary>
        /// Get gradient between primary and secondary colors
        /// </summary>
        public Gradient GetPrimaryGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(CurrentTheme.primaryColor, 0f),
                    new GradientColorKey(CurrentTheme.secondaryColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            return gradient;
        }

        /// <summary>
        /// Get rainbow gradient for special effects
        /// </summary>
        public static Gradient GetRainbowGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.25f),
                    new GradientColorKey(Color.green, 0.5f),
                    new GradientColorKey(Color.cyan, 0.75f),
                    new GradientColorKey(Color.magenta, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            return gradient;
        }

        /// <summary>
        /// Add a new theme at runtime
        /// </summary>
        public void AddTheme(ColorTheme theme)
        {
            if (availableThemes == null)
            {
                availableThemes = new ColorTheme[] { theme };
            }
            else
            {
                Array.Resize(ref availableThemes, availableThemes.Length + 1);
                availableThemes[availableThemes.Length - 1] = theme;
            }
        }

        /// <summary>
        /// Clear all themes
        /// </summary>
        public void ClearThemes()
        {
            availableThemes = null;
            CurrentTheme = null;
        }
    }
}
