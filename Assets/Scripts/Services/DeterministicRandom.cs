using System;
using UnityEngine;

namespace ChainReactionConveyor.Services
{
    /// <summary>
    /// Deterministic random number generator for reproducible gameplay
    /// </summary>
    public class DeterministicRandom
    {
        private int _seed;
        private System.Random _random;

        public DeterministicRandom(int seed)
        {
            SetSeed(seed);
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
            _random = new System.Random(seed);
            UnityEngine.Debug.Log($"[DeterministicRandom] Seed set to: {seed}");
        }

        public int GetSeed() => _seed;

        /// <summary>
        /// Returns random int between min (inclusive) and max (exclusive)
        /// </summary>
        public int Range(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Returns random float between min (inclusive) and max (inclusive)
        /// </summary>
        public float Range(float min, float max)
        {
            return (float)(_random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Returns random value from weighted table
        /// </summary>
        public int WeightedChoice(float[] weights)
        {
            float total = 0f;
            foreach (var w in weights)
            {
                total += w;
            }

            float value = (float)(_random.NextDouble() * total);
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (value < cumulative)
                {
                    return i;
                }
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// Returns true with given probability
        /// </summary>
        public bool Chance(float probability)
        {
            return _random.NextDouble() < probability;
        }

        /// <summary>
        /// Shuffle array in place using Fisher-Yates
        /// </summary>
        public void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        /// <summary>
        /// Create a seeded sub-generator for isolated random streams
        /// </summary>
        public DeterministicRandom Split(int offset = 0)
        {
            int newSeed = Range(int.MinValue, int.MaxValue);
            return new DeterministicRandom(newSeed);
        }

        public void Replay(int[] inputs, Action<int, int> onInput)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                onInput(i, inputs[i]);
            }
        }
    }

    /// <summary>
    /// Static helper for global deterministic random access
    /// </summary>
    public static class RNG
    {
        private static DeterministicRandom _global;

        public static void Initialize(int seed)
        {
            _global = new DeterministicRandom(seed);
        }

        public static int Range(int min, int max) => _global.Range(min, max);
        public static float Range(float min, float max) => _global.Range(min, max);
        public static bool Chance(float probability) => _global.Chance(probability);
        public static int WeightedChoice(float[] weights) => _global.WeightedChoice(weights);

        public static void Shuffle<T>(T[] array) => _global.Shuffle(array);
        public static DeterministicRandom Split() => _global.Split();
    }
}
