using UnityEngine;
using System;
using System.Collections.Generic;
using ChainReactionConveyor.Models;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Mechanics
{
    /// <summary>
    /// ConveyorMechanic - manages item spawning, movement, and pocket system
    /// </summary>
    public class ConveyorMechanic : BaseMechanic
    {
        [Header("Settings")]
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private float conveyorSpeed = 1.0f;
        [SerializeField] private int conveyorCapacity = 10;
        [SerializeField] private int pocketCount = 5;
        [SerializeField] private int pocketCapacity = 3;
        [SerializeField] private int maxSpawn = 50;

        [Header("Item Types")]
        [SerializeField] private int itemTypeCount = 4;
        [SerializeField] private float[] spawnWeights = { 0.25f, 0.25f, 0.25f, 0.25f };

        private Queue<int> _conveyorQueue = new();
        private List<int>[] _pocketSlots;
        private int _totalSpawned = 0;
        private float _spawnTimer = 0f;
        private bool _isFull = false;

        private DeterministicRandom _rng;

        public event Action<int> OnItemSpawned;
        public event Action<int, int> OnItemMovedToPocket; // itemId, pocketIndex
        public event Action<int> OnItemRouted; // pocketIndex
        public event Action OnConveyorFull;
        public event Action OnPocketOverflow;

        public override void Initialize()
        {
            base.Initialize();

            _rng = new DeterministicRandom(0); // Will be set from level seed
            _conveyorQueue.Clear();
            _pocketSlots = new List<int>[pocketCount];

            for (int i = 0; i < pocketCount; i++)
            {
                _pocketSlots[i] = new List<int>();
            }

            _totalSpawned = 0;
            _spawnTimer = 0f;
            _isFull = false;
        }

        public void SetSeed(int seed)
        {
            if (_rng == null) _rng = new DeterministicRandom(seed);
            else _rng.SetSeed(seed);
        }

        public void Configure(LevelDef levelDef)
        {
            spawnInterval = levelDef.spawnInterval;
            conveyorSpeed = levelDef.conveyorSpeed;
            conveyorCapacity = levelDef.maxSpawn;
            pocketCount = levelDef.pocketCount;
            pocketCapacity = levelDef.pocketCapacity;
            maxSpawn = levelDef.maxSpawn;

            Debug.Log($"[ConveyorMechanic] Configured - spawn: {spawnInterval}s, speed: {conveyorSpeed}");
        }

        public override void OnLevelStart()
        {
            base.OnLevelStart();
            _spawnTimer = 0f;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!_isPlaying || _isFull || _totalSpawned >= maxSpawn) return;

            _spawnTimer += deltaTime;

            if (_spawnTimer >= spawnInterval)
            {
                SpawnItem();
                _spawnTimer = 0f;
            }
        }

        private void SpawnItem()
        {
            if (_conveyorQueue.Count >= conveyorCapacity)
            {
                _isFull = true;
                OnConveyorFull?.Invoke();
                Publish(new ChainReactionConveyor.Services.ChainResolvedEvent { ClusterSize = -1 });
                return;
            }

            int itemType = _rng.WeightedChoice(spawnWeights);
            _conveyorQueue.Enqueue(itemType);
            _totalSpawned++;

            Debug.Log($"[ConveyorMechanic] Spawned item type {itemType}, queue: {_conveyorQueue.Count}");
            OnItemSpawned?.Invoke(itemType);
        }

        /// <summary>
        /// Route next item from conveyor to pocket
        /// </summary>
        public bool RouteToPocket(int pocketIndex)
        {
            if (!_isPlaying || pocketIndex < 0 || pocketIndex >= pocketCount)
                return false;

            if (_conveyorQueue.Count == 0)
                return false;

            int item = _conveyorQueue.Dequeue();

            // Check pocket capacity
            if (_pocketSlots[pocketIndex].Count >= pocketCapacity)
            {
                // Overflow!
                OnPocketOverflow?.Invoke();
                Publish(new LevelFailEvent { LevelId = 0, Reason = "Pocket overflow" });
                return false;
            }

            _pocketSlots[pocketIndex].Add(item);
            _isFull = false; // Not full anymore

            Debug.Log($"[ConveyorMechanic] Routed item to pocket {pocketIndex}, queue: {_conveyorQueue.Count}");
            OnItemRouted?.Invoke(pocketIndex);

            // Publish event for chain reaction
            Publish(new ItemPlacedEvent { X = pocketIndex, Y = 0, ItemId = item });

            return true;
        }

        /// <summary>
        /// Get item from pocket without using it (peek)
        /// </summary>
        public int PeekPocket(int pocketIndex)
        {
            if (pocketIndex < 0 || pocketIndex >= pocketCount)
                return -1;

            if (_pocketSlots[pocketIndex].Count == 0)
                return -1;

            return _pocketSlots[pocketIndex][0];
        }

        /// <summary>
        /// Use/remove item from pocket
        /// </summary>
        public int UsePocketItem(int pocketIndex)
        {
            if (pocketIndex < 0 || pocketIndex >= pocketCount)
                return -1;

            if (_pocketSlots[pocketIndex].Count == 0)
                return -1;

            int item = _pocketSlots[pocketIndex][0];
            _pocketSlots[pocketIndex].RemoveAt(0);

            return item;
        }

        /// <summary>
        /// Manual re-enqueue - return pocket item to conveyor
        /// </summary>
        public bool ReenqueuePocketItem(int pocketIndex)
        {
            if (pocketIndex < 0 || pocketIndex >= pocketCount)
                return false;

            if (_pocketSlots[pocketIndex].Count == 0)
                return false;

            if (_conveyorQueue.Count >= conveyorCapacity)
                return false;

            int item = _pocketSlots[pocketIndex][0];
            _pocketSlots[pocketIndex].RemoveAt(0);
            _conveyorQueue.Enqueue(item);

            Debug.Log($"[ConveyorMechanic] Re-enqueued pocket {pocketIndex} item");
            return true;
        }

        public int GetConveyorCount() => _conveyorQueue.Count;
        public int GetPocketCount(int pocketIndex) => pocketIndex >= 0 && pocketIndex < pocketCount ? _pocketSlots[pocketIndex].Count : 0;
        public int GetTotalSpawned() => _totalSpawned;
        public bool IsFull() => _isFull;

        public override void Shutdown()
        {
            _conveyorQueue.Clear();
            for (int i = 0; i < pocketCount; i++)
            {
                _pocketSlots[i]?.Clear();
            }
            base.Shutdown();
        }
    }
}
