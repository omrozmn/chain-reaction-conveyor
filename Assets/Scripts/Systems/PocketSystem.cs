using System.Collections.Generic;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Represents a pocket on the conveyor belt that can hold items
    /// </summary>
    public class Pocket
    {
        public int pocketId;
        public List<int> items;
        public int capacity;
        public bool isFull => items.Count >= capacity;
        public int Count => items.Count;

        public Pocket(int id, int capacity)
        {
            pocketId = id;
            this.capacity = capacity;
            items = new List<int>();
        }

        public bool CanAddItem()
        {
            return items.Count < capacity;
        }

        public bool AddItem(int itemId)
        {
            if (!CanAddItem())
            {
                return false;
            }
            items.Add(itemId);
            return true;
        }

        public bool RemoveItem(int itemId)
        {
            return items.Remove(itemId);
        }

        public void Clear()
        {
            items.Clear();
        }

        public int PeekTopItem()
        {
            if (items.Count == 0) return -1;
            return items[items.Count - 1];
        }

        public int PopTopItem()
        {
            if (items.Count == 0) return -1;
            int item = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            return item;
        }
    }

    /// <summary>
    /// Manages all pockets on the conveyor
    /// </summary>
    public class PocketSystem
    {
        private List<Pocket> _pockets;
        private int _pocketCapacity;

        public PocketSystem(int pocketCount, int pocketCapacity)
        {
            _pocketCapacity = pocketCapacity;
            _pockets = new List<Pocket>();
            
            for (int i = 0; i < pocketCount; i++)
            {
                _pockets.Add(new Pocket(i, pocketCapacity));
            }
        }

        public Pocket GetPocket(int index)
        {
            if (index < 0 || index >= _pockets.Count) return null;
            return _pockets[index];
        }

        public int GetPocketCount() => _pockets.Count;

        public int GetTotalCapacity() => _pockets.Count * _pocketCapacity;

        public int GetTotalItems()
        {
            int total = 0;
            foreach (var pocket in _pockets)
            {
                total += pocket.Count;
            }
            return total;
        }

        public bool IsOverflowing()
        {
            // Overflow = all pockets full
            foreach (var pocket in _pockets)
            {
                if (!pocket.isFull) return false;
            }
            return _pockets.Count > 0;
        }

        public bool HasAvailablePocket()
        {
            foreach (var pocket in _pockets)
            {
                if (pocket.CanAddItem()) return true;
            }
            return false;
        }

        public int GetAvailableSlotCount()
        {
            int available = 0;
            foreach (var pocket in _pockets)
            {
                available += (pocket.capacity - pocket.Count);
            }
            return available;
        }

        /// <summary>
        /// Try to add item to any available pocket
        /// </summary>
        /// <returns>True if item was added, false if overflow</returns>
        public bool TryAddItem(int itemId)
        {
            foreach (var pocket in _pockets)
            {
                if (pocket.CanAddItem())
                {
                    pocket.AddItem(itemId);
                    return true;
                }
            }
            return false; // Overflow!
        }

        /// <summary>
        /// Get the next pocket that will be filled (for prediction)
        /// </summary>
        public Pocket GetNextFullPocket()
        {
            foreach (var pocket in _pockets)
            {
                if (!pocket.isFull) return pocket;
            }
            return null; // All full
        }

        public void ClearAll()
        {
            foreach (var pocket in _pockets)
            {
                pocket.Clear();
            }
        }

        public Pocket[] GetAllPockets() => _pockets.ToArray();
    }
}
