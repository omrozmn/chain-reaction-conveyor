using UnityEngine;
using System.Collections.Generic;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Particle effect presets and configurations for the game
    /// </summary>
    [System.Serializable]
    public class ParticlePreset
    {
        public string name;
        public ParticleSystem particleSystem;
        
        [Header("Basic Settings")]
        public int maxParticles = 50;
        public float startLifetime = 1f;
        public float startSpeed = 2f;
        public float startSize = 0.5f;
        
        [Header("Emission")]
        public float emissionRate = 10f;
        public int bursts = 0;
        public int burstCount = 10;
        
        [Header("Shape")]
        public ParticleSystemShapeType shape = ParticleSystemShapeType.Sphere;
        public float shapeRadius = 0.5f;
        
        [Header("Colors")]
        public Color startColor = Color.white;
        public Color endColor = Color.clear;
        
        [Header("Movement")]
        public Vector3 velocityOverLifetime = Vector3.zero;
        public bool randomizeRotation = true;
    }

    public enum ParticleEffectType
    {
        ClusterPop,
        ScoreGain,
        BoosterActivate,
        ConveyorMove,
        ComboStreak,
        ItemPlace,
        GameOver,
        LevelComplete
    }

    /// <summary>
    /// Manages particle effects throughout the game
    /// </summary>
    public class ParticleEffectsManager : MonoBehaviour
    {
        public static ParticleEffectsManager Instance { get; private set; }

        [Header("Particle Presets")]
        [SerializeField] private ParticlePreset clusterPopPreset;
        [SerializeField] private ParticlePreset scoreGainPreset;
        [SerializeField] private ParticlePreset boosterActivatePreset;
        [SerializeField] private ParticlePreset comboStreakPreset;

        [Header("Pool Settings")]
        [SerializeField] private int poolSize = 20;
        [SerializeField] private bool autoExpand = true;

        private Dictionary<ParticleEffectType, Queue<ParticleSystem>> particlePools;
        private Dictionary<ParticleEffectType, ParticlePreset> presets;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
            InitializePresets();
        }

        private void InitializePools()
        {
            particlePools = new Dictionary<ParticleEffectType, Queue<ParticleSystem>>();
            
            foreach (ParticleEffectType type in System.Enum.GetValues(typeof(ParticleEffectType)))
            {
                particlePools[type] = new Queue<ParticleSystem>();
            }
        }

        private void InitializePresets()
        {
            presets = new Dictionary<ParticleEffectType, ParticlePreset>
            {
                { ParticleEffectType.ClusterPop, clusterPopPreset },
                { ParticleEffectType.ScoreGain, scoreGainPreset },
                { ParticleEffectType.BoosterActivate, boosterActivatePreset },
                { ParticleEffectType.ComboStreak, comboStreakPreset }
            };
        }

        /// <summary>
        /// Play particle effect at position
        /// </summary>
        public void PlayEffect(ParticleEffectType type, Vector3 position, Color? customColor = null)
        {
            ParticleSystem ps = GetParticleSystem(type);
            if (ps == null)
            {
                // Create temporary particle system
                ps = CreateTempParticleSystem(type);
            }

            // Apply custom color if provided
            if (customColor.HasValue)
            {
                var main = ps.main;
                main.startColor = customColor.Value;
            }

            ps.transform.position = position;
            ps.Play();

            // Return to pool after duration
            StartCoroutine(ReturnToPool(type, ps, ps.main.startLifetime.constantMax + 0.5f));
        }

        /// <summary>
        /// Play particle effect with custom color from theme
        /// </summary>
        public void PlayEffectWithTheme(ParticleEffectType type, Vector3 position, BoosterType? boosterType = null)
        {
            Color? customColor = null;
            
            if (boosterType.HasValue && ColorsManager.Instance != null)
            {
                customColor = ColorsManager.Instance.GetBoosterColor(boosterType.Value);
            }
            else if (type == ParticleEffectType.ScoreGain && ColorsManager.Instance != null)
            {
                customColor = ColorsManager.Instance.CurrentTheme?.successColor;
            }
            else if (type == ParticleEffectType.ComboStreak && ColorsManager.Instance != null)
            {
                customColor = ColorsManager.Instance.CurrentTheme?.accentColor;
            }

            PlayEffect(type, position, customColor);
        }

        private ParticleSystem GetParticleSystem(ParticleEffectType type)
        {
            if (particlePools == null || !particlePools.ContainsKey(type))
                return null;

            var pool = particlePools[type];
            
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            if (autoExpand && presets.ContainsKey(type) && presets[type] != null)
            {
                return CreateParticleFromPreset(presets[type]);
            }

            return null;
        }

        private ParticleSystem CreateTempParticleSystem(ParticleEffectType type)
        {
            GameObject go = new GameObject("TempParticle_" + type);
            var ps = go.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 20;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            return ps;
        }

        private ParticleSystem CreateParticleFromPreset(ParticlePreset preset)
        {
            if (preset == null || preset.particleSystem == null)
                return CreateTempParticleSystem(ParticleEffectType.ClusterPop);

            return Instantiate(preset.particleSystem, transform);
        }

        private System.Collections.IEnumerator ReturnToPool(ParticleEffectType type, ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                ps.Clear();
                
                if (particlePools.ContainsKey(type))
                {
                    particlePools[type].Enqueue(ps);
                }
                else
                {
                    Destroy(ps.gameObject);
                }
            }
        }

        /// <summary>
        /// Create particle system configuration for Cluster Pop effect
        /// </summary>
        public static ParticleSystem CreateClusterPopEffect()
        {
            GameObject go = new GameObject("ClusterPop");
            var ps = go.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = Color.white;
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 1f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0f, 15, 20) 
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            return ps;
        }

        /// <summary>
        /// Create particle system configuration for Score Float effect
        /// </summary>
        public static ParticleSystem CreateScoreFloatEffect()
        {
            GameObject go = new GameObject("ScoreFloat");
            var ps = go.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 0.8f;
            main.startSpeed = 1f;
            main.startSize = 0.2f;
            main.startColor = Color.yellow;
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = -0.5f; // Float upward

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0f, 5, 8) 
            });

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            return ps;
        }

        /// <summary>
        /// Create particle system configuration for Booster Activate effect
        /// </summary>
        public static ParticleSystem CreateBoosterActivateEffect()
        {
            GameObject go = new GameObject("BoosterActivate");
            var ps = go.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 0.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = 0.15f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { 
                new ParticleSystem.Burst(0f, 20, 30) 
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.5f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.renderMode = ParticleSystemRenderMode.Stretch;

            return ps;
        }
    }
}
