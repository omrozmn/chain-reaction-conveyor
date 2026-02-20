using UnityEngine;
using System;

namespace ChainReactionConveyor.Mechanics
{
    /// <summary>
    /// Interface for objects that can participate in chain reactions.
    /// Implemented by items that trigger or respond to chain reactions on the conveyor.
    /// </summary>
    public interface IChainReactable
    {
        /// <summary>
        /// Unique identifier for this reactable item
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// The type/category of this item (used for cluster matching)
        /// </summary>
        int ItemType { get; }
        
        /// <summary>
        /// Whether this item is currently active in the chain reaction system
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Called when this item is triggered as part of a chain reaction
        /// </summary>
        void OnChainTriggered();
        
        /// <summary>
        /// Called when this item completes its chain reaction
        /// </summary>
        void OnChainComplete();
        
        /// <summary>
        /// Check if this item can react with another reactable
        /// </summary>
        bool CanReactWith(IChainReactable other);
        
        /// <summary>
        /// Get the position in grid coordinates
        /// </summary>
        Vector2Int GetGridPosition();
        
        /// <summary>
        /// Set the position in grid coordinates
        /// </summary>
        void SetGridPosition(Vector2Int position);
    }
    
    /// <summary>
    /// Event data for chain reaction events
    /// </summary>
    public struct ChainReactionEvent
    {
        public string ReactableId;
        public Vector2Int Position;
        public int ItemType;
        public float TriggerTime;
    }
}
