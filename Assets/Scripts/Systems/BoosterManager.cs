using UnityEngine;
using System;
using System.Collections.Generic;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Booster types available in the game
    /// </summary>
    public enum BoosterType
    {
        Swap,   // Swap two adjacent items
        Bomb,   // Destroy items in a radius
        Slow    // Slow down conveyor for limited time
    }

    /// <summary>
    /// Event data for booster activation
    /// </summary>
    public class BoosterActivatedEvent
    {
        public BoosterType Type { get; set; }
        public Vector2 Position { get; set; }
        public int Charges { get; set; }
    }

    /// <summary>
    /// Event data for booster inventory changes
    /// </summary>
    public class BoosterInventoryChangedEvent
    {
        public BoosterType Type { get; set; }
        public int Charges { get; set; }
        public int MaxCharges { get; set; }
    }

    /// <summary>
    /// Manages all boosters in the game - creation, activation, inventory, and refill mechanics
    /// Uses IComponent pattern for Unity integration
    /// </summary>
    public class BoosterManager : MonoBehaviour, IEventSubscriber
    {
        public static BoosterManager Instance { get; private set; }

        [Header("Booster Inventory")]
        [SerializeField] private int swapCharges = 3;
        [SerializeField] private int bombCharges = 2;
        [SerializeField] private int slowCharges = 2;

        [Header("Booster Settings")]
        [SerializeField] private float bombRadius = 2f;
        [SerializeField] private float slowDuration = 5f;
        [SerializeField] private float slowFactor = 0.3f;

        [Header("Booster Refill Mechanics")]
        [SerializeField] private bool enableRefillOnLevelComplete = true;
        [SerializeField] private int refillAmountPerLevel = 1;
        [SerializeField] private bool enablePeriodicRefill = true;
        [SerializeField] private float refillIntervalSeconds = 60f;
        [SerializeField] private int maxChargesSwap = 5;
        [SerializeField] private int maxChargesBomb = 3;
        [SerializeField] private int maxChargesSlow = 3;

        #region IEventSubscriber

        public void Subscribe()
        {
            EventBus.Instance.Subscribe<LevelCompleteEvent>(OnLevelComplete);
            EventBus.Instance.Subscribe<LevelStartEvent>(OnLevelStart);
        }

        public void Unsubscribe()
        {
            EventBus.Instance.Clear<LevelCompleteEvent>();
            EventBus.Instance.Clear<LevelStartEvent>();
        }

        #endregion

        // Active booster instances
        private Dictionary<BoosterType, int> boosterInventory = new Dictionary<BoosterType, int>();
        private Dictionary<BoosterType, int> maxCharges = new Dictionary<BoosterType, int>();
        private bool isSlowActive = false;
        private float slowTimer = 0f;
        private float refillTimer = 0f;

        // Events
        public event Action<BoosterActivatedEvent> OnBoosterActivated;
        public event Action<BoosterType, int> OnInventoryChanged;
        public event Action<BoosterType> OnBoosterRefilled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
            InitializeMaxCharges();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void InitializeInventory()
        {
            boosterInventory[BoosterType.Swap] = swapCharges;
            boosterInventory[BoosterType.Bomb] = bombCharges;
            boosterInventory[BoosterType.Slow] = slowCharges;
        }

        private void InitializeMaxCharges()
        {
            maxCharges[BoosterType.Swap] = maxChargesSwap;
            maxCharges[BoosterType.Bomb] = maxChargesBomb;
            maxCharges[BoosterType.Slow] = maxChargesSlow;
        }

        private void Update()
        {
            HandleSlowEffect();
            HandlePeriodicRefill();
        }

        private void HandleSlowEffect()
        {
            if (isSlowActive)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f)
                {
                    EndSlowEffect();
                }
            }
        }

        private void HandlePeriodicRefill()
        {
            if (!enablePeriodicRefill) return;

            refillTimer += Time.deltaTime;
            if (refillTimer >= refillIntervalSeconds)
            {
                refillTimer = 0f;
                RefillAllBoosters();
            }
        }

        private void OnLevelComplete(LevelCompleteEvent evt)
        {
            if (enableRefillOnLevelComplete)
            {
                RefillAllBoosters();
            }
        }

        private void OnLevelStart(LevelStartEvent evt)
        {
            ResetBoosters();
        }

        /// <summary>
        /// Refill all boosters by refillAmountPerLevel
        /// </summary>
        public void RefillAllBoosters()
        {
            foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
            {
                RefillBooster(type, refillAmountPerLevel);
            }
            Debug.Log("[BoosterManager] All boosters refilled!");
            
            // Publish to EventBus
            EventBus.Instance.Publish(new BoosterInventoryChangedEvent
            {
                Type = BoosterType.Swap,
                Charges = GetCharges(BoosterType.Swap),
                MaxCharges = GetMaxCharges(BoosterType.Swap)
            });
        }

        /// <summary>
        /// Refill a specific booster type
        /// </summary>
        public void RefillBooster(BoosterType type, int amount)
        {
            if (!maxCharges.ContainsKey(type)) return;

            int current = boosterInventory.TryGetValue(type, out int c) ? c : 0;
            int max = maxCharges[type];
            
            if (current < max)
            {
                int newAmount = Mathf.Min(current + amount, max);
                boosterInventory[type] = newAmount;
                
                OnInventoryChanged?.Invoke(type, newAmount);
                OnBoosterRefilled?.Invoke(type);
                
                // Publish to EventBus
                EventBus.Instance.Publish(new BoosterInventoryChangedEvent
                {
                    Type = type,
                    Charges = newAmount,
                    MaxCharges = max
                });
                
                Debug.Log($"[BoosterManager] Refilled {type}: {current} -> {newAmount}");
            }
        }

        /// <summary>
        /// Get current charges for a booster type
        /// </summary>
        public int GetCharges(BoosterType type)
        {
            return boosterInventory.TryGetValue(type, out int charges) ? charges : 0;
        }

        /// <summary>
        /// Get max charges for a booster type
        /// </summary>
        public int GetMaxCharges(BoosterType type)
        {
            return maxCharges.TryGetValue(type, out int max) ? max : 0;
        }

        /// <summary>
        /// Check if a booster is available
        /// </summary>
        public bool HasBooster(BoosterType type)
        {
            return GetCharges(type) > 0;
        }

        /// <summary>
        /// Check if booster can be refilled
        /// </summary>
        public bool CanRefill(BoosterType type)
        {
            int current = GetCharges(type);
            int max = GetMaxCharges(type);
            return current < max;
        }

        /// <summary>
        /// Activate a booster at given position
        /// </summary>
        public bool ActivateBooster(BoosterType type, Vector2 position)
        {
            if (!HasBooster(type))
            {
                Debug.LogWarning($"[BoosterManager] No charges left for {type}");
                return false;
            }

            // Deduct charge
            boosterInventory[type]--;
            OnInventoryChanged?.Invoke(type, boosterInventory[type]);

            // Publish to EventBus
            EventBus.Instance.Publish(new BoosterInventoryChangedEvent
            {
                Type = type,
                Charges = boosterInventory[type],
                MaxCharges = GetMaxCharges(type)
            });

            // Execute booster effect
            switch (type)
            {
                case BoosterType.Swap:
                    ExecuteSwap(position);
                    break;
                case BoosterType.Bomb:
                    ExecuteBomb(position);
                    break;
                case BoosterType.Slow:
                    ExecuteSlow();
                    break;
            }

            // Fire local event
            OnBoosterActivated?.Invoke(new BoosterActivatedEvent
            {
                Type = type,
                Position = position,
                Charges = boosterInventory[type]
            });

            // Publish to EventBus
            EventBus.Instance.Publish(new BoosterUsedEvent
            {
                BoosterType = type.ToString()
            });

            Debug.Log($"[BoosterManager] Activated {type} at {position}, remaining: {boosterInventory[type]}");
            return true;
        }

        private void ExecuteSwap(Vector2 position)
        {
            // Find adjacent items and swap them
            var items = FindItemsInRadius(position, 1.5f);
            if (items.Count >= 2)
            {
                var item1 = items[0];
                var item2 = items[1];
                var pos1 = item1.transform.position;
                var pos2 = item2.transform.position;
                item1.transform.position = pos2;
                item2.transform.position = pos1;
                Debug.Log($"[BoosterManager] Swapped items at {pos1} and {pos2}");
            }
            else
            {
                Debug.Log($"[BoosterManager] Not enough items to swap at {position}");
            }
        }

        private void ExecuteBomb(Vector2 position)
        {
            var items = FindItemsInRadius(position, bombRadius);
            foreach (var item in items)
            {
                var chainable = item.GetComponent<Mechanics.IChainReactable>();
                if (chainable != null)
                {
                    chainable.TriggerChainReaction();
                }
                else
                {
                    Destroy(item.gameObject);
                }
            }
            Debug.Log($"[BoosterManager] Bomb destroyed {items.Count} items at {position}");
        }

        private void ExecuteSlow()
        {
            if (isSlowActive)
            {
                slowTimer = slowDuration;
            }
            else
            {
                StartSlowEffect();
            }
        }

        private void StartSlowEffect()
        {
            isSlowActive = true;
            slowTimer = slowDuration;
            Time.timeScale = slowFactor;
            Debug.Log($"[BoosterManager] Slow effect started for {slowDuration}s");
        }

        private void EndSlowEffect()
        {
            isSlowActive = false;
            Time.timeScale = 1f;
            Debug.Log("[BoosterManager] Slow effect ended");
        }

        private List<MonoBehaviour> FindItemsInRadius(Vector2 position, float radius)
        {
            var items = new List<MonoBehaviour>();
            var colliders = Physics2D.OverlapCircleAll(position, radius);
            foreach (var collider in colliders)
            {
                var item = collider.GetComponent<MonoBehaviour>();
                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        public void AddCharges(BoosterType type, int amount)
        {
            if (boosterInventory.ContainsKey(type))
            {
                int max = GetMaxCharges(type);
                boosterInventory[type] = Mathf.Min(boosterInventory[type] + amount, max);
            }
            else
            {
                boosterInventory[type] = Mathf.Min(amount, GetMaxCharges(type));
            }
            OnInventoryChanged?.Invoke(type, boosterInventory[type]);
            
            // Publish to EventBus
            EventBus.Instance.Publish(new BoosterInventoryChangedEvent
            {
                Type = type,
                Charges = boosterInventory[type],
                MaxCharges = GetMaxCharges(type)
            });
        }

        public void ResetBoosters()
        {
            InitializeInventory();
            isSlowActive = false;
            slowTimer = 0f;
            refillTimer = 0f;
            Time.timeScale = 1f;
            
            // Notify via local events
            OnInventoryChanged?.Invoke(BoosterType.Swap, swapCharges);
            OnInventoryChanged?.Invoke(BoosterType.Bomb, bombCharges);
            OnInventoryChanged?.Invoke(BoosterType.Slow, slowCharges);
            
            // Notify via EventBus
            foreach (BoosterType type in Enum.GetValues(typeof(BoosterType)))
            {
                EventBus.Instance.Publish(new BoosterInventoryChangedEvent
                {
                    Type = type,
                    Charges = GetCharges(type),
                    MaxCharges = GetMaxCharges(type)
                });
            }
        }

        public float GetSlowRemainingTime() => isSlowActive ? slowTimer : 0f;
        public bool IsSlowActive() => isSlowActive;
        
        /// <summary>
        /// Get time until next refill
        /// </summary>
        public float GetTimeUntilRefill() => enablePeriodicRefill ? Mathf.Max(0, refillIntervalSeconds - refillTimer) : 0;
    }
}
