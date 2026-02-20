using NUnit.Framework;
using System;
using System.Collections.Generic;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for EventBus
    /// </summary>
    [TestFixture]
    public class EventBusTest
    {
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = EventBus.Instance;
            _eventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _eventBus.Clear();
        }

        #region Subscribe Tests

        [Test]
        public void Subscribe_AddsListener()
        {
            bool eventReceived = false;
            Action<TestEvent> listener = e => eventReceived = true;

            _eventBus.Subscribe(listener);

            _eventBus.Publish(new TestEvent { Value = 42 });

            Assert.That(eventReceived, Is.True);
        }

        [Test]
        public void Subscribe_MultipleListeners_AllCalled()
        {
            int callCount = 0;
            Action<TestEvent> listener1 = e => callCount++;
            Action<TestEvent> listener2 = e => callCount++;

            _eventBus.Subscribe(listener1);
            _eventBus.Subscribe(listener2);

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(callCount, Is.EqualTo(2));
        }

        [Test]
        public void Subscribe_DifferentEventTypes_NotCalled()
        {
            bool otherEventReceived = false;
            Action<OtherTestEvent> listener = e => otherEventReceived = true;

            _eventBus.Subscribe(listener);

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(otherEventReceived, Is.False);
        }

        #endregion

        #region Unsubscribe Tests

        [Test]
        public void Unsubscribe_RemovesListener()
        {
            bool eventReceived = false;
            Action<TestEvent> listener = e => eventReceived = true;

            _eventBus.Subscribe(listener);
            _eventBus.Unsubscribe(listener);

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(eventReceived, Is.False);
        }

        [Test]
        public void Unsubscribe_OtherListenersStillReceive()
        {
            int callCount = 0;
            Action<TestEvent> listener1 = e => callCount++;
            Action<TestEvent> listener2 = e => callCount++;

            _eventBus.Subscribe(listener1);
            _eventBus.Subscribe(listener2);
            _eventBus.Unsubscribe(listener1);

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(callCount, Is.EqualTo(1));
        }

        #endregion

        #region SubscribeOnce Tests

        [Test]
        public void SubscribeOnce_ReceivesEventOnce()
        {
            int callCount = 0;
            Action<TestEvent> listener = e => callCount++;

            _eventBus.SubscribeOnce(listener);

            _eventBus.Publish(new TestEvent { Value = 1 });
            _eventBus.Publish(new TestEvent { Value = 2 });

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void SubscribeOnce_MultipleOnceListeners_AllReceiveOnce()
        {
            int callCount = 0;
            Action<TestEvent> listener1 = e => callCount++;
            Action<TestEvent> listener2 = e => callCount++;

            _eventBus.SubscribeOnce(listener1);
            _eventBus.SubscribeOnce(listener2);

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(callCount, Is.EqualTo(2));
        }

        [Test]
        public void SubscribeOnce_MixedWithPermanent_OnceListenerRemoved()
        {
            int permanentCount = 0;
            int onceCount = 0;
            Action<TestEvent> permanentListener = e => permanentCount++;
            Action<TestEvent> onceListener = e => onceCount++;

            _eventBus.Subscribe(permanentListener);
            _eventBus.SubscribeOnce(onceListener);

            _eventBus.Publish(new TestEvent { Value = 1 });
            _eventBus.Publish(new TestEvent { Value = 2 });

            Assert.That(permanentCount, Is.EqualTo(2));
            Assert.That(onceCount, Is.EqualTo(1));
        }

        #endregion

        #region Publish Tests

        [Test]
        public void Publish_PassesEventData()
        {
            TestEvent receivedEvent = null;
            Action<TestEvent> listener = e => receivedEvent = e;

            _eventBus.Subscribe(listener);
            _eventBus.Publish(new TestEvent { Value = 123 });

            Assert.That(receivedEvent, Is.Not.Null);
            Assert.That(receivedEvent.Value, Is.EqualTo(123));
        }

        [Test]
        public void Publish_NoListeners_NoException()
        {
            Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent { Value = 1 }));
        }

        [Test]
        public void Publish_StructEvent_CopiesData()
        {
            TestEvent? receivedEvent = null;
            Action<TestEvent> listener = e => receivedEvent = e;

            var originalEvent = new TestEvent { Value = 999 };
            _eventBus.Subscribe(listener);
            _eventBus.Publish(originalEvent);

            // Modify original after publish
            originalEvent.Value = 0;

            Assert.That(receivedEvent.Value, Is.EqualTo(999));
        }

        #endregion

        #region Clear Tests

        [Test]
        public void Clear_RemovesAllListeners()
        {
            bool eventReceived = false;
            Action<TestEvent> listener = e => eventReceived = true;

            _eventBus.Subscribe(listener);
            _eventBus.Clear();

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(eventReceived, Is.False);
        }

        [Test]
        public void Clear_RemovesAllOnceListeners()
        {
            bool eventReceived = false;
            Action<TestEvent> listener = e => eventReceived = true;

            _eventBus.SubscribeOnce(listener);
            _eventBus.Clear();

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(eventReceived, Is.False);
        }

        [Test]
        public void ClearType_RemovesSpecificEventType()
        {
            bool testEventReceived = false;
            bool otherEventReceived = false;
            Action<TestEvent> testListener = e => testEventReceived = true;
            Action<OtherTestEvent> otherListener = e => otherEventReceived = true;

            _eventBus.Subscribe(testListener);
            _eventBus.Subscribe(otherListener);

            _eventBus.Clear<TestEvent>();

            _eventBus.Publish(new TestEvent { Value = 1 });
            _eventBus.Publish(new OtherTestEvent { Name = "test" });

            Assert.That(testEventReceived, Is.False);
            Assert.That(otherEventReceived, Is.True);
        }

        #endregion

        #region Game Event Tests

        [Test]
        public void LevelStartEvent_PublishesCorrectly()
        {
            int receivedLevelId = -1;
            int receivedSeed = -1;

            _eventBus.Subscribe<LevelStartEvent>(e =>
            {
                receivedLevelId = e.LevelId;
                receivedSeed = e.Seed;
            });

            _eventBus.Publish(new LevelStartEvent { LevelId = 5, Seed = 12345 });

            Assert.That(receivedLevelId, Is.EqualTo(5));
            Assert.That(receivedSeed, Is.EqualTo(12345));
        }

        [Test]
        public void LevelCompleteEvent_PublishesCorrectly()
        {
            int receivedLevelId = -1;
            int receivedScore = -1;

            _eventBus.Subscribe<LevelCompleteEvent>(e =>
            {
                receivedLevelId = e.LevelId;
                receivedScore = e.Score;
            });

            _eventBus.Publish(new LevelCompleteEvent { LevelId = 10, Score = 5000 });

            Assert.That(receivedLevelId, Is.EqualTo(10));
            Assert.That(receivedScore, Is.EqualTo(5000));
        }

        [Test]
        public void ItemPlacedEvent_PublishesCorrectly()
        {
            int receivedX = -1;
            int receivedY = -1;
            int receivedItemId = -1;

            _eventBus.Subscribe<ItemPlacedEvent>(e =>
            {
                receivedX = e.X;
                receivedY = e.Y;
                receivedItemId = e.ItemId;
            });

            _eventBus.Publish(new ItemPlacedEvent { X = 3, Y = 5, ItemId = 7 });

            Assert.That(receivedX, Is.EqualTo(3));
            Assert.That(receivedY, Is.EqualTo(5));
            Assert.That(receivedItemId, Is.EqualTo(7));
        }

        [Test]
        public void ChainResolvedEvent_PublishesCorrectly()
        {
            int clusterSize = 0;
            int chainDepth = 0;

            _eventBus.Subscribe<ChainResolvedEvent>(e =>
            {
                clusterSize = e.ClusterSize;
                chainDepth = e.ChainDepth;
            });

            _eventBus.Publish(new ChainResolvedEvent { ClusterSize = 10, ChainDepth = 3 });

            Assert.That(clusterSize, Is.EqualTo(10));
            Assert.That(chainDepth, Is.EqualTo(3));
        }

        #endregion

        #region Edge Cases

        [Test]
        public void Subscribe_SameListenerTwice_AddsOnlyOnce()
        {
            int callCount = 0;
            Action<TestEvent> listener = e => callCount++;

            _eventBus.Subscribe(listener);
            _eventBus.Subscribe(listener);

            _eventBus.Publish(new TestEvent { Value = 1 });

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Unsubscribe_NotSubscribedListener_NoException()
        {
            Action<TestEvent> listener = e => { };

            Assert.DoesNotThrow(() => _eventBus.Unsubscribe(listener));
        }

        [Test]
        public void Publish_NullEvent_NoException()
        {
            Action<NullableEvent> listener = e => { };

            _eventBus.Subscribe(listener);

            Assert.DoesNotThrow(() => _eventBus.Publish(new NullableEvent()));
        }

        #endregion

        #region Test Event Types

        private struct TestEvent
        {
            public int Value;
        }

        private struct OtherTestEvent
        {
            public string Name;
        }

        private struct NullableEvent
        {
        }

        #endregion
    }
}
