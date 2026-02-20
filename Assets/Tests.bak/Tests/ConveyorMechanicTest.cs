using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Services;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for ConveyorMechanic
    /// </summary>
    [TestFixture]
    public class ConveyorMechanicTest
    {
        private GameObject _gameObject;
        private Mechanics.ConveyorMechanic _mechanic;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("ConveyorMechanicTest");
            _mechanic = _gameObject.AddComponent<Mechanics.ConveyorMechanic>();
            
            // Initialize mechanic
            _mechanic.Initialize();
            _mechanic.SetSeed(42);
        }

        [TearDown]
        public void TearDown()
        {
            _mechanic.Shutdown();
            GameObject.DestroyImmediate(_gameObject);
            EventBus.Instance.Clear();
        }

        #region Initialize Tests

        [Test]
        public void Initialize_SetsDefaultValues()
        {
            Assert.That(_mechanic.GetConveyorCount(), Is.EqualTo(0));
            Assert.That(_mechanic.GetTotalSpawned(), Is.EqualTo(0));
            Assert.That(_mechanic.IsFull(), Is.False);
        }

        [Test]
        public void SetSeed_SetsRandomSeed()
        {
            _mechanic.SetSeed(123);
            
            // No exception means success
            Assert.Pass();
        }

        #endregion

        #region Configure Tests

        [Test]
        public void Configure_SetsLevelDefValues()
        {
            var levelDef = new LevelDef
            {
                levelId = 1,
                seed = 100,
                spawnInterval = 2.0f,
                conveyorSpeed = 1.5f,
                maxSpawn = 20,
                pocketCount = 6,
                pocketCapacity = 4
            };

            _mechanic.Configure(levelDef);

            // Trigger update to spawn
            _mechanic.OnUpdate(2.1f);

            Assert.That(_mechanic.GetTotalSpawned(), Is.GreaterThan(0));
        }

        #endregion

        #region Spawn Tests

        [Test]
        public void OnUpdate_SpawnsItem_AfterInterval()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f); // spawnInterval is 1.5f

            Assert.That(_mechanic.GetConveyorCount(), Is.EqualTo(1));
            Assert.That(_mechanic.GetTotalSpawned(), Is.EqualTo(1));
        }

        [Test]
        public void OnUpdate_DoesNotSpawn_BeforeInterval()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.0f);

            Assert.That(_mechanic.GetConveyorCount(), Is.EqualTo(0));
        }

        [Test]
        public void OnUpdate_SpawnsMultipleItems()
        {
            _mechanic.OnLevelStart();
            
            // Spawn 3 items
            _mechanic.OnUpdate(1.6f);
            _mechanic.OnUpdate(1.6f);
            _mechanic.OnUpdate(1.6f);

            Assert.That(_mechanic.GetTotalSpawned(), Is.EqualTo(3));
        }

        [Test]
        public void OnUpdate_ResetsTimer_AfterSpawn()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(3.0f); // Should spawn once, not twice

            Assert.That(_mechanic.GetTotalSpawned(), Is.EqualTo(1));
        }

        [Test]
        public void OnUpdate_RespectsMaxSpawn()
        {
            _mechanic.SetSeed(0);
            _mechanic.OnLevelStart();

            // Spawn more than maxSpawn (default 50)
            for (int i = 0; i < 60; i++)
            {
                _mechanic.OnUpdate(2.0f);
            }

            Assert.That(_mechanic.GetTotalSpawned(), Is.LessThanOrEqualTo(50));
        }

        #endregion

        #region RouteToPocket Tests

        [Test]
        public void RouteToPocket_RemovesFromConveyor()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f); // Spawn item

            bool result = _mechanic.RouteToPocket(0);

            Assert.That(result, Is.True);
            Assert.That(_mechanic.GetConveyorCount(), Is.EqualTo(0));
        }

        [Test]
        public void RouteToPocket_AddsToPocket()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);

            _mechanic.RouteToPocket(2);

            Assert.That(_mechanic.GetPocketCount(2), Is.EqualTo(1));
        }

        [Test]
        public void RouteToPocket_InvalidIndex_ReturnsFalse()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);

            bool result = _mechanic.RouteToPocket(-1);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RouteToPocket_OutOfRangeIndex_ReturnsFalse()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);

            bool result = _mechanic.RouteToPocket(100);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RouteToPocket_EmptyConveyor_ReturnsFalse()
        {
            bool result = _mechanic.RouteToPocket(0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RouteToPocket_PocketOverflow_TriggersFail()
        {
            _mechanic.SetSeed(0);
            _mechanic.OnLevelStart();

            // Fill pocket to capacity (default 3)
            _mechanic.OnUpdate(1.6f); _mechanic.RouteToPocket(0);
            _mechanic.OnUpdate(1.6f); _mechanic.RouteToPocket(0);
            _mechanic.OnUpdate(1.6f); _mechanic.RouteToPocket(0);

            // Try to add 4th item - should fail
            _mechanic.OnUpdate(1.6f);
            bool result = _mechanic.RouteToPocket(0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void RouteToPocket_ResetsFullFlag()
        {
            _mechanic.SetSeed(0);
            _mechanic.OnLevelStart();

            // Fill conveyor to capacity (default 10)
            for (int i = 0; i < 10; i++)
            {
                _mechanic.OnUpdate(2.0f);
            }

            Assert.That(_mechanic.IsFull(), Is.True);

            // Route an item
            _mechanic.RouteToPocket(0);

            Assert.That(_mechanic.IsFull(), Is.False);
        }

        #endregion

        #region Pocket Operations Tests

        [Test]
        public void PeekPocket_ReturnsFirstItem()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);
            _mechanic.RouteToPocket(0);

            int item = _mechanic.PeekPocket(0);

            Assert.That(item, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void PeekPocket_EmptyPocket_ReturnsMinusOne()
        {
            int item = _mechanic.PeekPocket(0);

            Assert.That(item, Is.EqualTo(-1));
        }

        [Test]
        public void PeekPocket_InvalidIndex_ReturnsMinusOne()
        {
            int item = _mechanic.PeekPocket(-1);

            Assert.That(item, Is.EqualTo(-1));
        }

        [Test]
        public void UsePocketItem_RemovesItem()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);
            _mechanic.RouteToPocket(0);

            _mechanic.UsePocketItem(0);

            Assert.That(_mechanic.GetPocketCount(0), Is.EqualTo(0));
        }

        [Test]
        public void UsePocketItem_ReturnsItem()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);
            _mechanic.RouteToPocket(0);

            int item = _mechanic.UsePocketItem(0);

            Assert.That(item, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void UsePocketItem_EmptyPocket_ReturnsMinusOne()
        {
            int item = _mechanic.UsePocketItem(0);

            Assert.That(item, Is.EqualTo(-1));
        }

        [Test]
        public void ReenqueuePocketItem_ReturnsToConveyor()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);
            _mechanic.RouteToPocket(0);

            bool result = _mechanic.ReenqueuePocketItem(0);

            Assert.That(result, Is.True);
            Assert.That(_mechanic.GetPocketCount(0), Is.EqualTo(0));
            Assert.That(_mechanic.GetConveyorCount(), Is.EqualTo(1));
        }

        [Test]
        public void ReenqueuePocketItem_EmptyPocket_ReturnsFalse()
        {
            bool result = _mechanic.ReenqueuePocketItem(0);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ReenqueuePocketItem_FullConveyor_ReturnsFalse()
        {
            _mechanic.SetSeed(0);
            _mechanic.OnLevelStart();

            // Fill conveyor
            for (int i = 0; i < 10; i++)
            {
                _mechanic.OnUpdate(2.0f);
            }

            // Add item to pocket
            _mechanic.OnUpdate(2.0f);
            _mechanic.RouteToPocket(0);

            // Try to reenqueue - should fail because conveyor is full
            bool result = _mechanic.ReenqueuePocketItem(0);

            Assert.That(result, Is.False);
        }

        #endregion

        #region Event Tests

        [Test]
        public void OnItemSpawned_EventFires()
        {
            bool eventFired = false;
            _mechanic.OnItemSpawned += type => eventFired = true;

            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnItemRouted_EventFires()
        {
            bool eventFired = false;
            int pocketIndex = -1;
            _mechanic.OnItemRouted += index => { eventFired = true; pocketIndex = index; };

            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);
            _mechanic.RouteToPocket(2);

            Assert.That(eventFired, Is.True);
            Assert.That(pocketIndex, Is.EqualTo(2));
        }

        [Test]
        public void OnConveyorFull_EventFires()
        {
            bool eventFired = false;
            _mechanic.OnConveyorFull += () => eventFired = true;
            _mechanic.SetSeed(0);
            _mechanic.OnLevelStart();

            // Fill conveyor to capacity
            for (int i = 0; i < 10; i++)
            {
                _mechanic.OnUpdate(2.0f);
            }

            Assert.That(eventFired, Is.True);
        }

        #endregion

        #region Shutdown Tests

        [Test]
        public void Shutdown_ClearsAllData()
        {
            _mechanic.OnLevelStart();
            _mechanic.OnUpdate(1.6f);
            _mechanic.RouteToPocket(0);

            _mechanic.Shutdown();

            Assert.That(_mechanic.GetConveyorCount(), Is.EqualTo(0));
            Assert.That(_mechanic.GetPocketCount(0), Is.EqualTo(0));
        }

        #endregion
    }
}
