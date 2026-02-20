using NUnit.Framework;
using UnityEngine;
using ChainReactionConveyor.Services;
using ChainReactionConveyor.Models;

namespace ChainReactionConveyor.Tests
{
    /// <summary>
    /// Unit tests for ChainResolver
    /// </summary>
    [TestFixture]
    public class ChainResolverTest
    {
        private Systems.ChainResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _resolver = new Systems.ChainResolver();
            _resolver.Initialize(6, 8, 3); // 6x8 board, min cluster 3
        }

        [TearDown]
        public void TearDown()
        {
            _resolver = null;
        }

        #region Initialize Tests

        [Test]
        public void Initialize_SetsDimensions()
        {
            var (width, height) = _resolver.GetDimensions();

            Assert.That(width, Is.EqualTo(6));
            Assert.That(height, Is.EqualTo(8));
        }

        [Test]
        public void Clear_EmptiesBoard()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.Clear();

            Assert.That(_resolver.GetItemCount(), Is.EqualTo(0));
        }

        #endregion

        #region PlaceItem Tests

        [Test]
        public void PlaceItem_AddsItem()
        {
            _resolver.PlaceItem(0, 0, 1);

            Assert.That(_resolver.GetItem(0, 0), Is.EqualTo(1));
        }

        [Test]
        public void PlaceItem_InvalidPosition_ReturnsZero()
        {
            int result = _resolver.PlaceItem(-1, 0, 1);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void PlaceItem_OutOfRange_ReturnsZero()
        {
            int result = _resolver.PlaceItem(10, 10, 1);

            Assert.That(result, Is.EqualTo(0));
        }

        #endregion

        #region Cluster Detection Tests

        [Test]
        public void PlaceItem_SingleItem_NoCluster()
        {
            int clusterSize = _resolver.PlaceItem(0, 0, 1);

            Assert.That(clusterSize, Is.EqualTo(0));
        }

        [Test]
        public void PlaceItem_TwoAdjacent_NoCluster()
        {
            _resolver.PlaceItem(0, 0, 1);
            int clusterSize = _resolver.PlaceItem(1, 0, 1);

            Assert.That(clusterSize, Is.EqualTo(0));
        }

        [Test]
        public void PlaceItem_ThreeHorizontal_FormsCluster()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            int clusterSize = _resolver.PlaceItem(2, 0, 1);

            Assert.That(clusterSize, Is.EqualTo(3));
        }

        [Test]
        public void PlaceItem_ThreeVertical_FormsCluster()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(0, 1, 1);
            int clusterSize = _resolver.PlaceItem(0, 2, 1);

            Assert.That(clusterSize, Is.EqualTo(3));
        }

        [Test]
        public void PlaceItem_DifferentTypes_DoNotCluster()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            int clusterSize = _resolver.PlaceItem(2, 0, 2); // Different type

            Assert.That(clusterSize, Is.EqualTo(0));
        }

        [Test]
        public void PlaceItem_LShaped_FormsCluster()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            _resolver.PlaceItem(0, 1, 1);
            int clusterSize = _resolver.PlaceItem(0, 2, 1);

            // Should detect cluster of 4 (L-shape)
            Assert.That(clusterSize, Is.EqualTo(4));
        }

        [Test]
        public void PlaceItem_Box_FormsCluster()
        {
            // 2x2 box
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            _resolver.PlaceItem(0, 1, 1);
            int clusterSize = _resolver.PlaceItem(1, 1, 1);

            Assert.That(clusterSize, Is.EqualTo(4));
        }

        #endregion

        #region Cluster Event Tests

        [Test]
        public void OnClusterFound_EventFires()
        {
            bool eventFired = false;
            _resolver.OnClusterFound += cluster => eventFired = true;

            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, _resolver.Place 1);
           Item(2, 0, 1);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void OnClusterFound_ContainsCorrectPositions()
        {
            System.Collections.Generic.List<Vector2Int> capturedCluster = null;
            _resolver.OnClusterFound += cluster => capturedCluster = cluster;

            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            _resolver.PlaceItem(2, 0, 1);

            Assert.That(capturedCluster, Is.Not.Null);
            Assert.That(capturedCluster.Count, Is.EqualTo(3));
        }

        #endregion

        #region RemoveCluster Tests

        [Test]
        public void RemoveCluster_ClearsPositions()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            _resolver.PlaceItem(2, 0, 1);

            var cluster = new System.Collections.Generic.List<Vector2Int> {
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            };
            _resolver.RemoveCluster(cluster);

            Assert.That(_resolver.GetItem(0, 0), Is.EqualTo(-1));
            Assert.That(_resolver.GetItem(1, 0), Is.EqualTo(-1));
            Assert.That(_resolver.GetItem(2, 0), Is.EqualTo(-1));
        }

        #endregion

        #region Position Validation Tests

        [Test]
        public void IsValidPosition_Valid_ReturnsTrue()
        {
            Assert.That(_resolver.IsValidPosition(0, 0), Is.True);
            Assert.That(_resolver.IsValidPosition(5, 7), Is.True);
        }

        [Test]
        public void IsValidPosition_Invalid_ReturnsFalse()
        {
            Assert.That(_resolver.IsValidPosition(-1, 0), Is.False);
            Assert.That(_resolver.IsValidPosition(0, -1), Is.False);
            Assert.That(_resolver.IsValidPosition(6, 0), Is.False);
            Assert.That(_resolver.IsValidPosition(0, 8), Is.False);
        }

        [Test]
        public void IsEmpty_EmptyPosition_ReturnsTrue()
        {
            Assert.That(_resolver.IsEmpty(0, 0), Is.True);
        }

        [Test]
        public void IsEmpty_OccupiedPosition_ReturnsFalse()
        {
            _resolver.PlaceItem(0, 0, 1);

            Assert.That(_resolver.IsEmpty(0, 0), Is.False);
        }

        #endregion

        #region GetEmptyPositions Tests

        [Test]
        public void GetEmptyPositions_ReturnsAllPositions()
        {
            var empty = _resolver.GetEmptyPositions();

            Assert.That(empty.Count, Is.EqualTo(48)); // 6x8 = 48
        }

        [Test]
        public void GetEmptyPositions_ExcludesOccupied()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);

            var empty = _resolver.GetEmptyPositions();

            Assert.That(empty.Count, Is.EqualTo(46));
        }

        #endregion

        #region GetItemCount Tests

        [Test]
        public void GetItemCount_EmptyBoard_ReturnsZero()
        {
            Assert.That(_resolver.GetItemCount(), Is.EqualTo(0));
        }

        [Test]
        public void GetItemCount_ReturnsCorrectCount()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            _resolver.PlaceItem(2, 0, 2);

            Assert.That(_resolver.GetItemCount(), Is.EqualTo(3));
        }

        #endregion

        #region Edge Cases

        [Test]
        public void PlaceItem_SamePosition_Overwrites()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(0, 0, 2);

            Assert.That(_resolver.GetItem(0, 0), Is.EqualTo(2));
        }

        [Test]
        public void Initialize_ResetsProcessedPositions()
        {
            _resolver.PlaceItem(0, 0, 1);
            _resolver.PlaceItem(1, 0, 1);
            _resolver.PlaceItem(2, 0, 1);

            _resolver.Initialize(6, 8, 3);

            Assert.That(_resolver.GetItemCount(), Is.EqualTo(0));
        }

        #endregion
    }
}
