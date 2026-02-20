using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// ChainResolver - handles cluster detection and chain reaction logic using BFS
    /// </summary>
    public class ChainResolver
    {
        private int[,] _board;
        private int _width;
        private int _height;
        private int _minCluster;
        
        // Cluster tracking
        private HashSet<Vector2Int> _processedPositions = new();
        
        // Events
        public event Action<List<Vector2Int>> OnClusterFound;
        public event Action<int> OnChainResolved; // Total items resolved

        public void Initialize(int width, int height, int minCluster = 3)
        {
            _width = width;
            _height = height;
            _minCluster = minCluster;
            _board = new int[width, height];
            Clear();
            
            Debug.Log($"[ChainResolver] Initialized - Board: {width}x{height}, MinCluster: {minCluster}");
        }

        public void Clear()
        {
            if (_board != null)
            {
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        _board[x, y] = -1; // -1 = empty
                    }
                }
            }
            _processedPositions.Clear();
        }

        /// <summary>
        /// Place an item on the board and check for clusters
        /// </summary>
        /// <returns>Number of items in the cluster if found, 0 otherwise</returns>
        public int PlaceItem(int x, int y, int itemType)
        {
            if (!IsValidPosition(x, y))
            {
                Debug.LogWarning($"[ChainResolver] Invalid position: ({x}, {y})");
                return 0;
            }

            _board[x, y] = itemType;
            
            // Check for cluster using BFS
            var cluster = FindCluster(x, y, itemType);
            
            if (cluster.Count >= _minCluster)
            {
                Debug.Log($"[ChainResolver] Cluster found! Size: {cluster.Count}, Type: {itemType}");
                OnClusterFound?.Invoke(cluster);
                return cluster.Count;
            }
            
            return 0;
        }

        /// <summary>
        /// Find all connected items of the same type using BFS
        /// </summary>
        private List<Vector2Int> FindCluster(int startX, int startY, int targetType)
        {
            var cluster = new List<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            Vector2Int start = new Vector2Int(startX, startY);
            queue.Enqueue(start);
            visited.Add(start);

            // 4-directional adjacency (up, down, left, right)
            Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                cluster.Add(current);

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    if (IsValidPosition(neighbor.x, neighbor.y) &&
                        !visited.Contains(neighbor) &&
                        _board[neighbor.x, neighbor.y] == targetType)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return cluster;
        }

        /// <summary>
        /// Remove a cluster from the board
        /// </summary>
        public void RemoveCluster(List<Vector2Int> cluster)
        {
            foreach (var pos in cluster)
            {
                _board[pos.x, pos.y] = -1;
            }
            Debug.Log($"[ChainResolver] Removed cluster of size {cluster.Count}");
        }

        /// <summary>
        /// Get item at position
        /// </summary>
        public int GetItem(int x, int y)
        {
            if (!IsValidPosition(x, y)) return -1;
            return _board[x, y];
        }

        /// <summary>
        /// Check if position is valid
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        /// <summary>
        /// Get board dimensions
        /// </summary>
        public (int width, int height) GetDimensions() => (_width, _height);

        /// <summary>
        /// Check if position is empty
        /// </summary>
        public bool IsEmpty(int x, int y)
        {
            if (!IsValidPosition(x, y)) return false;
            return _board[x, y] == -1;
        }

        /// <summary>
        /// Get all empty positions
        /// </summary>
        public List<Vector2Int> GetEmptyPositions()
        {
            var empty = new List<Vector2Int>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_board[x, y] == -1)
                    {
                        empty.Add(new Vector2Int(x, y));
                    }
                }
            }
            return empty;
        }

        /// <summary>
        /// Get count of items on board
        /// </summary>
        public int GetItemCount()
        {
            int count = 0;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_board[x, y] != -1) count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Debug: Print board state
        /// </summary>
        public void PrintBoard()
        {
            string boardStr = "[ChainResolver] Board:\n";
            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    int val = _board[x, y];
                    boardStr += val == -1 ? ". " : $"{val} ";
                }
                boardStr += "\n";
            }
            Debug.Log(boardStr);
        }
    }
}
