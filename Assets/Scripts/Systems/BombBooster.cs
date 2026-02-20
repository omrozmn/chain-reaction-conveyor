using UnityEngine;
using System;
using System.Collections.Generic;

namespace ChainReactionConveyor.Systems
{
    /// <summary>
    /// Bomb Booster - Destroys items in a radius and triggers chain reactions
    /// </summary>
    [RequireComponent(typeof(BoosterManager))]
    public class BombBooster : MonoBehaviour
    {
        [Header("Bomb Settings")]
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float explosionDelay = 0.1f;
        [SerializeField] private Color explosionColor = Color.red;
        [SerializeField] private float explosionAnimationDuration = 0.5f;

        // Reference to BoosterManager
        private BoosterManager boosterManager;

        // Events
        public event Action<Vector2, int> OnBombExplode;
        public event Action OnBombFailed;

        private void Awake()
        {
            boosterManager = GetComponent<BoosterManager>();
        }

        /// <summary>
        /// Activate bomb at position - destroys all items in radius
        /// </summary>
        public bool Activate(Vector2 position)
        {
            if (!boosterManager.HasBooster(BoosterType.Bomb))
            {
                Debug.LogWarning("[BombBooster] No bomb charges available");
                OnBombFailed?.Invoke();
                return false;
            }

            // Find items in blast radius
            var items = FindItemsInRadius(position, explosionRadius);

            if (items.Count == 0)
            {
                Debug.LogWarning($"[BombBooster] No items in blast radius at {position}");
                OnBombFailed?.Invoke();
                return false;
            }

            // Deduct charge first
            boosterManager.ActivateBooster(BoosterType.Bomb, position);

            // Start explosion sequence
            StartCoroutine(ExplosionSequence(position, items));

            OnBombExplode?.Invoke(position, items.Count);
            return true;
        }

        private List<Transform> FindItemsInRadius(Vector2 center, float radius)
        {
            var items = new List<Transform>();
            var colliders = Physics2D.OverlapCircleAll(center, radius);
            
            foreach (var collider in colliders)
            {
                // Filter out non-destroyable objects
                if (collider.CompareTag("Item") || collider.GetComponent<MonoBehaviour>() != null)
                {
                    items.Add(collider.transform);
                }
            }
            
            return items;
        }

        private System.Collections.IEnumerator ExplosionSequence(Vector2 position, List<Transform> items)
        {
            // Visual feedback - could spawn particle effect here
            yield return new WaitForSeconds(explosionDelay);

            int destroyedCount = 0;

            foreach (var item in items)
            {
                if (item == null) continue;

                // Check for chain reaction capability
                var chainable = item.GetComponent<Mechanics.IChainReactable>();
                
                if (chainable != null)
                {
                    // Trigger chain reaction instead of direct destroy
                    chainable.TriggerChainReaction();
                }
                else
                {
                    // Direct destroy
                    Destroy(item.gameObject);
                }
                
                destroyedCount++;
            }

            Debug.Log($"[BombBooster] Explosion at {position} destroyed {destroyedCount} items");
        }

        /// <summary>
        /// Get items that would be affected (for UI preview)
        /// </summary>
        public int GetAffectedItemCount(Vector2 position)
        {
            return FindItemsInRadius(position, explosionRadius).Count;
        }

        /// <summary>
        /// Preview explosion radius (for UI)
        /// </summary>
        public void DrawExplosionPreview(Vector2 position, float duration = 1f)
        {
            // Visual debug - can be used for UI overlay
            StartCoroutine(DrawRadiusPreview(position, duration));
        }

        private System.Collections.IEnumerator DrawRadiusPreview(Vector2 position, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                // Debug visualization (DrawWireSphere not available in Unity)
                // Debug.DrawWireSphere(new Vector3(position.x, position.y, 0), Vector3.zero, explosionRadius);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Get the explosion radius
        /// </summary>
        public float GetExplosionRadius() => explosionRadius;

        /// <summary>
        /// Set explosion radius (for upgrades)
        /// </summary>
        public void SetExplosionRadius(float radius)
        {
            explosionRadius = Mathf.Max(0.5f, radius);
        }
    }
}
