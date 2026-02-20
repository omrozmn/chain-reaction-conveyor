using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Manages all visual feedback effects in the game
    /// </summary>
    public class VisualFeedbackSystem : MonoBehaviour
    {
        public static VisualFeedbackSystem Instance { get; private set; }

        [Header("Effect Prefabs")]
        [SerializeField] private GameObject clusterPopPrefab;
        [SerializeField] private GameObject scoreFloatPrefab;
        [SerializeField] private GameObject comboTextPrefab;
        [SerializeField] private GameObject boosterActivatePrefab;

        [Header("Effect Settings")]
        [SerializeField] private float popDuration = 0.5f;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float floatHeight = 1.5f;
        [SerializeField] private float comboScaleTime = 0.3f;

        [Header("Screen Shake")]
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.2f;

        [Header("Color Flash")]
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private Color successFlashColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color failFlashColor = new Color(1, 0, 0, 0.3f);

        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Image screenFlashImage;

        private Vector3 originalCameraPosition;
        private bool isShaking = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
                originalCameraPosition = mainCamera.transform.position;
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            SubscribeToThemeChanges();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            UnsubscribeFromThemeChanges();
        }

        private void SubscribeToThemeChanges()
        {
            if (ColorsManager.Instance != null)
            {
                ColorsManager.Instance.OnThemeChanged += OnThemeChanged;
                ColorsManager.Instance.OnThemeApplied += OnThemeApplied;
            }
        }

        private void UnsubscribeFromThemeChanges()
        {
            if (ColorsManager.Instance != null)
            {
                ColorsManager.Instance.OnThemeChanged -= OnThemeChanged;
                ColorsManager.Instance.OnThemeApplied -= OnThemeApplied;
            }
        }

        private void OnThemeChanged(ColorTheme theme)
        {
            // Could update effect colors here when theme changes
            Debug.Log("[VisualFeedback] Theme changed to: " + theme.themeName);
        }

        private void OnThemeApplied()
        {
            // Could refresh effect materials here
            Debug.Log("[VisualFeedback] Theme applied");
        }

        private void SubscribeToEvents()
        {
            var resolver = FindFirstObjectByType<ChainResolver>();
            if (resolver != null)
            {
                resolver.OnClusterFound += OnClusterFound;
            }
        }

        private void UnsubscribeFromEvents()
        {
            var resolver = FindFirstObjectByType<ChainResolver>();
            if (resolver != null)
            {
                resolver.OnClusterFound -= OnClusterFound;
            }
        }

        private void OnClusterFound(List<Vector2Int> cluster)
        {
            StartCoroutine(ClusterEffectSequence(cluster));
        }

        private IEnumerator ClusterEffectSequence(List<Vector2Int> cluster)
        {
            int shakeIntensityMod = Mathf.Clamp(cluster.Count / 3, 1, 5);
            TriggerScreenShake(shakeIntensity * shakeIntensityMod);
            TriggerFlash(true);
            yield return new WaitForSeconds(0.1f);
            TriggerFlash(false);
        }

        public void TriggerScreenShake(float intensity = -1)
        {
            if (isShaking) return;
            StartCoroutine(ScreenShakeCoroutine(intensity > 0 ? intensity : shakeIntensity));
        }

        private IEnumerator ScreenShakeCoroutine(float intensity)
        {
            isShaking = true;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                if (mainCamera == null) break;
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                mainCamera.transform.position = originalCameraPosition + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (mainCamera != null)
                mainCamera.transform.position = originalCameraPosition;
            isShaking = false;
        }

        public void TriggerFlash(bool success)
        {
            StartCoroutine(FlashCoroutine(success));
        }

        private IEnumerator FlashCoroutine(bool success)
        {
            if (screenFlashImage == null) yield break;
            Color targetColor = success ? successFlashColor : failFlashColor;
            screenFlashImage.color = targetColor;
            screenFlashImage.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                screenFlashImage.color = Color.Lerp(targetColor, Color.clear, elapsed / flashDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            screenFlashImage.gameObject.SetActive(false);
        }

        public void ShowFloatingText(string text, Vector3 worldPosition, Color color, float size = 1f)
        {
            if (scoreFloatPrefab == null) return;
            GameObject floatObj = Instantiate(scoreFloatPrefab, worldPosition, Quaternion.identity);
            var textComponent = floatObj.GetComponent<TextMesh>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = color;
                textComponent.characterSize *= size;
            }
            StartCoroutine(FloatingTextRoutine(floatObj));
        }

        public void ShowScoreFloat(int score, Vector3 worldPosition)
        {
            Color scoreColor = ColorsManager.Instance?.CurrentTheme?.successColor ?? Color.green;
            ShowFloatingText("+" + score, worldPosition, scoreColor, 1.2f);
        }

        public void ShowComboText(int combo, Vector3 worldPosition)
        {
            Color comboColor = ColorsManager.Instance?.CurrentTheme?.accentColor ?? Color.yellow;
            ShowFloatingText("x" + combo + " COMBO!", worldPosition, comboColor, 1.5f);
        }

        private IEnumerator FloatingTextRoutine(GameObject obj)
        {
            float elapsed = 0f;
            Vector3 startPos = obj.transform.position;
            Vector3 endPos = startPos + Vector3.up * floatHeight;

            while (elapsed < popDuration)
            {
                if (obj == null) yield break;
                float t = elapsed / popDuration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f);
                obj.transform.position = Vector3.Lerp(startPos, endPos, t);
                var textMesh = obj.GetComponent<TextMesh>();
                if (textMesh != null)
                {
                    Color c = textMesh.color;
                    c.a = 1f - (elapsed / popDuration);
                    textMesh.color = c;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }

        public void TriggerButtonPress(Button button)
        {
            if (button == null) return;
            StartCoroutine(ButtonPressRoutine(button));
        }

        private IEnumerator ButtonPressRoutine(Button button)
        {
            Vector3 originalScale = button.transform.localScale;
            float elapsed = 0f;
            while (elapsed < comboScaleTime / 2)
            {
                float t = elapsed / (comboScaleTime / 2);
                button.transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < comboScaleTime / 2)
            {
                float t = elapsed / (comboScaleTime / 2);
                button.transform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            button.transform.localScale = originalScale;
        }

        public void ShowBoosterActivation(BoosterType type, Vector3 position)
        {
            if (boosterActivatePrefab == null) return;
            Color boosterColor = ColorsManager.Instance?.GetBoosterColor(type) ?? Color.white;
            GameObject effect = Instantiate(boosterActivatePrefab, position, Quaternion.identity);
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = boosterColor;
            }
            Destroy(effect, 2f);
        }
    }
}
