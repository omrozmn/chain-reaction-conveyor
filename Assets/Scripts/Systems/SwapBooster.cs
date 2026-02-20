using UnityEngine;
using System;
using System.Collections.Generic;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Swap Booster - Swaps two adjacent items on the conveyor
    /// </summary>
    [RequireComponent(typeof(BoosterManager))]
    public class SwapBooster : MonoBehaviour
    {
        [Header("Swap Settings")]
        [SerializeField] private float swapRadius = 1.5f;
        [SerializeField] private float animationDuration = 0.3f;
        
        // Reference to BoosterManager
        private BoosterManager boosterManager;
        
        // Events
        public event Action<Vector2, Vector2> OnSwapComplete;
        public event Action OnSwapFailed;

        private void Awake()
        {
            boosterManager = GetComponent<BoosterManager>();
        }

        /// <summary>
        /// Activate swap at position - finds two closest items and swaps them
        /// </summary>
        public bool Activate(Vector2 position)
        {
            if (!boosterManager.HasBooster(BoosterType.Swap))
            {
                Debug.LogWarning("[SwapBooster] No swap charges available");
                OnSwapFailed?.Invoke();
                return false;
            }

            // Find items to swap
            var items = FindClosestItems(position, 2);
            
            if (items.Count < 2)
            {
                Debug.LogWarning($"[SwapBooster] Not enough items to swap at {position}");
                OnSwapFailed?.Invoke();
                return false;
            }

            // Perform swap
            Transform item1 = items[0];
            Transform item2 = items[1];
            
            Vector2 pos1 = item1.position;
            Vector2 pos2 = item2.position;

            // Animate swap
            StartCoroutine(SwapAnimation(item1, item2, pos1, pos2));
            
            // Deduct charge
            boosterManager.ActivateBooster(BoosterType.Swap, position);
            
            OnSwapComplete?.Invoke(pos1, pos2);
            return true;
        }

        private List<Transform> FindClosestItems(Vector2 center, int count)
        {
            var items = new List<Transform>();
            var colliders = Physics2D.OverlapCircleAll(center, swapRadius);
            
            // Sort by distance
            Array.Sort(colliders, (a, b) => 
                Vector2.Distance(center, a.transform.position)
                    .CompareTo(Vector2.Distance(center, b.transform.position)));
            
            // Get first 'count' items
            for (int i = 0; i < Mathf.Min(count, colliders.Length); i++)
            {
                items.Add(colliders[i].transform);
            }
            
            return items;
        }

        private System.Collections.IEnumerator SwapAnimation(Transform item1, Transform item2, Vector2 startPos1, Vector2 startPos2)
        {
            float elapsed = 0f;
            
            while (elapsed < animationDuration)
            {
                float t = elapsed / animationDuration;
                // Smooth step interpolation
                t = t * t * (3f - 2f * t);
                
                item1.position = Vector2.Lerp(startPos1, startPos2, t);
                item2.position = Vector2.Lerp(startPos2, startPos1, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Ensure final positions
            item1.position = startPos2;
            item2.position = startPos1;
        }

        /// <summary>
        /// Preview which items would be swapped (for UI)
        /// </summary>
        public (Transform, Transform)? GetSwapPreview(Vector2 position)
        {
            var items = FindClosestItems(position, 2);
            if (items.Count >= 2)
            {
                return (items[0], items[1]);
            }
            return null;
        }
    }
}
