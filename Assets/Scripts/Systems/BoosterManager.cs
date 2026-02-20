using UnityEngine;
using System;
using System.Collections.Generic;

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
    /// Manages all boosters in the game - creation, activation, and inventory
    /// </summary>
    public class BoosterManager : MonoBehaviour
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

        // Active booster instances
        private Dictionary<BoosterType, int> boosterInventory = new Dictionary<BoosterType, int>();
        private bool isSlowActive = false;
        private float slowTimer = 0f;

        // Events
        public event Action<BoosterActivatedEvent> OnBoosterActivated;
        public event Action<BoosterType, int> OnInventoryChanged;

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
        }

        private void InitializeInventory()
        {
            boosterInventory[BoosterType.Swap] = swapCharges;
            boosterInventory[BoosterType.Bomb] = bombCharges;
            boosterInventory[BoosterType.Slow] = slowCharges;
        }

        private void Update()
        {
            HandleSlowEffect();
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

        /// <summary>
        /// Get current charges for a booster type
        /// </summary>
        public int GetCharges(BoosterType type)
        {
            return boosterInventory.TryGetValue(type, out int charges) ? charges : 0;
        }

        /// <summary>
        /// Check if a booster is available
        /// </summary>
        public bool HasBooster(BoosterType type)
        {
            return GetCharges(type) > 0;
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

            // Fire event
            OnBoosterActivated?.Invoke(new BoosterActivatedEvent
            {
                Type = type,
                Position = position,
                Charges = boosterInventory[type]
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
                boosterInventory[type] += amount;
            }
            else
            {
                boosterInventory[type] = amount;
            }
            OnInventoryChanged?.Invoke(type, boosterInventory[type]);
        }

        public void ResetBoosters()
        {
            InitializeInventory();
            isSlowActive = false;
            slowTimer = 0f;
            Time.timeScale = 1f;
            OnInventoryChanged?.Invoke(BoosterType.Swap, swapCharges);
            OnInventoryChanged?.Invoke(BoosterType.Bomb, bombCharges);
            OnInventoryChanged?.Invoke(BoosterType.Slow, slowCharges);
        }

        public float GetSlowRemainingTime() => isSlowActive ? slowTimer : 0f;
        public bool IsSlowActive() => isSlowActive;
    }
}
