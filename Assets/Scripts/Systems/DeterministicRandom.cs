using UnityEngine;
using System.Collections.Generic;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Deterministic random number generator with seed support
    /// Ensures reproducible results for replay system
    /// </summary>
    public class DeterministicRandom
    {
        private int _seed;
        private int _state;

        public DeterministicRandom(int seed)
        {
            SetSeed(seed);
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
            _state = seed;
        }

        public int GetSeed() => _seed;

        /// <summary>
        /// Returns random float between 0 (inclusive) and 1 (exclusive)
        /// </summary>
        public float Value()
        {
            // Mulberry32 PRNG algorithm
            int t = _state += 0x6D2B79F5;
            t = (t ^ (t >> 15)) * (t | 1);
            t ^= t + (t ^ (t >> 7)) * (t | 61);
            _state = t;
            return ((t ^ (t >> 14)) >> 0) / 4294967296f;
        }

        /// <summary>
        /// Returns random int between 0 (inclusive) and max (exclusive)
        /// </summary>
        public int Range(int max)
        {
            return (int)(Value() * max);
        }

        /// <summary>
        /// Returns random int between min (inclusive) and max (exclusive)
        /// </summary>
        public int Range(int min, int max)
        {
            return min + Range(max - min);
        }

        /// <summary>
        /// Returns random float between min and max
        /// </summary>
        public float Range(float min, float max)
        {
            return min + Value() * (max - min);
        }

        /// <summary>
        /// Returns true with given probability (0-1)
        /// </summary>
        public bool Chance(float probability)
        {
            return Value() < probability;
        }

        /// <summary>
        /// Shuffle a list in-place using Fisher-Yates algorithm
        /// </summary>
        public void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Range(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Reset to original seed - enables replay
        /// </summary>
        public void Reset()
        {
            _state = _seed;
        }

        /// <summary>
        /// Get current state for debugging
        /// </summary>
        public int GetState() => _state;
    }
}
