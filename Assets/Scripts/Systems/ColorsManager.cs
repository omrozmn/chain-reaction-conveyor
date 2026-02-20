using UnityEngine;
using System;
using System.Collections.Generic;

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
    /// Manages all color themes in the game
    /// </summary>
    public class ColorsManager : MonoBehaviour
    {
        public static ColorsManager Instance { get; private set; }

        [Header("Themes")]
        [SerializeField] private ColorTheme[] availableThemes;
        [SerializeField] private int currentThemeIndex = 0;

        // Current theme
        public ColorTheme CurrentTheme { get; private set; }

        // Events
        public event Action<ColorTheme> OnThemeChanged;

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

            Debug.Log($"[ColorsManager] Loaded theme: {CurrentTheme.themeName}");
            OnThemeChanged?.Invoke(CurrentTheme);
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
        /// Get next theme in list
        /// </summary>
        public void NextTheme()
        {
            int nextIndex = (currentThemeIndex + 1) % availableThemes.Length;
            LoadTheme(nextIndex);
        }

        /// <summary>
        /// Get previous theme in list
        /// </summary>
        public void PreviousTheme()
        {
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
            var canvases = FindObjectsOfType<UnityEngine.Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.GetComponent<UnityEngine.UI.Image>().color = CurrentTheme.backgroundColor;
            }

            // Apply to all UI Images and Text
            var images = FindObjectsOfType<UnityEngine.UI.Image>();
            foreach (var image in images)
            {
                if (image.CompareTag("Themeable"))
                {
                    image.color = CurrentTheme.secondaryColor;
                }
            }

            var texts = FindObjectsOfType<UnityEngine.UI.Text>();
            foreach (var text in texts)
            {
                if (text.CompareTag("Themeable"))
                {
                    text.color = CurrentTheme.textColor;
                }
            }

            Debug.Log("[ColorsManager] Theme applied to UI");
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
    }
}
