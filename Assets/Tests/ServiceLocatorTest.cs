using NUnit.Framework;
using System;
using ChainReactionConveyor.Services;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for ServiceLocator
    /// </summary>
    [TestFixture]
    public class ServiceLocatorTest
    {
        private ServiceLocator _locator;

        [SetUp]
        public void SetUp()
        {
            _locator = ServiceLocator.Instance;
            _locator.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _locator.Clear();
        }

        #region Registration Tests

        [Test]
        public void Register_AddsServiceToLocator()
        {
            var testService = new TestService();

            _locator.Register<TestService>(testService);

            Assert.That(_locator.IsRegistered<TestService>(), Is.True);
        }

        [Test]
        public void Register_ReplacesExistingService()
        {
            var service1 = new TestService();
            var service2 = new TestService();

            _locator.Register<TestService>(service1);
            _locator.Register<TestService>(service2);

            var retrieved = _locator.Get<TestService>();
            Assert.That(retrieved, Is.EqualTo(service2));
        }

        #endregion

        #region Get Tests

        [Test]
        public void Get_ReturnsRegisteredService()
        {
            var testService = new TestService();
            _locator.Register<TestService>(testService);

            var result = _locator.Get<TestService>();

            Assert.That(result, Is.EqualTo(testService));
        }

        [Test]
        public void Get_ThrowsExceptionForUnregisteredService()
        {
            Assert.Throws<InvalidOperationException>(() => _locator.Get<TestService>());
        }

        [Test]
        public void GetOrNull_ReturnsNullForUnregisteredService()
        {
            var result = _locator.GetOrNull<TestService>();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetOrNull_ReturnsRegisteredService()
        {
            var testService = new TestService();
            _locator.Register<TestService>(testService);

            var result = _locator.GetOrNull<TestService>();

            Assert.That(result, Is.EqualTo(testService));
        }

        #endregion

        #region Factory Tests

        [Test]
        public void RegisterFactory_CreatesServiceOnFirstGet()
        {
            bool factoryCalled = false;
            _locator.RegisterFactory<TestService>(() =>
            {
                factoryCalled = true;
                return new TestService();
            });

            var result = _locator.Get<TestService>();

            Assert.That(factoryCalled, Is.True);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Get_CachesFactoryCreatedService()
        {
            int factoryCallCount = 0;
            _locator.RegisterFactory<TestService>(() =>
            {
                factoryCallCount++;
                return new TestService();
            });

            _locator.Get<TestService>();
            _locator.Get<TestService>();
            _locator.Get<TestService>();

            Assert.That(factoryCallCount, Is.EqualTo(1));
        }

        [Test]
        public void IsRegistered_ReturnsTrueForFactory()
        {
            _locator.RegisterFactory<TestService>(() => new TestService());

            Assert.That(_locator.IsRegistered<TestService>(), Is.True);
        }

        #endregion

        #region Unregister Tests

        [Test]
        public void Unregister_RemovesService()
        {
            var testService = new TestService();
            _locator.Register<TestService>(testService);

            _locator.Unregister<TestService>();

            Assert.That(_locator.IsRegistered<TestService>(), Is.False);
        }

        [Test]
        public void Unregister_ThrowsForServiceAfterUnregister()
        {
            var testService = new TestService();
            _locator.Register<TestService>(testService);
            _locator.Unregister<TestService>();

            Assert.Throws<InvalidOperationException>(() => _locator.Get<TestService>());
        }

        [Test]
        public void Clear_RemovesAllServices()
        {
            _locator.Register<TestService>(new TestService());
            _locator.Register<AnotherTestService>(new AnotherTestService());

            _locator.Clear();

            Assert.That(_locator.IsRegistered<TestService>(), Is.False);
            Assert.That(_locator.IsRegistered<AnotherTestService>(), Is.False);
        }

        #endregion

        #region Multiple Service Tests

        [Test]
        public void CanRegisterAndGetMultipleServices()
        {
            var service1 = new TestService();
            var service2 = new AnotherTestService();

            _locator.Register<TestService>(service1);
            _locator.Register<AnotherTestService>(service2);

            Assert.That(_locator.Get<TestService>(), Is.EqualTo(service1));
            Assert.That(_locator.Get<AnotherTestService>(), Is.EqualTo(service2));
        }

        #endregion

        /// <summary>
        /// Test service implementation
        /// </summary>
        private class TestService
        {
            public string Value { get; set; } = "test";
        }

        /// <summary>
        /// Another test service implementation
        /// </summary>
        private class AnotherTestService
        {
            public int Id { get; set; } = 42;
        }
    }
}
