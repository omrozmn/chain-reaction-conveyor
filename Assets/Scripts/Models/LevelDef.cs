using UnityEngine;

namespace ChainReactionConveyor.Models
{
    /// <summary>
    /// Level definition model - contains all parameters for a level
    /// </summary>
    [System.Serializable]
    public class LevelDef
    {
        public int levelId;
        public int seed;

        // Board settings
        public int boardWidth = 6;
        public int boardHeight = 8;

        // Gameplay settings
        public int minCluster = 3;
        public float spawnInterval = 1.5f;
        public float conveyorSpeed = 1.0f;

        // Target settings
        public int targetProgress = 20;
        public TargetType targetType = TargetType.FillSlots;

        // Difficulty flags
        public bool isSpike = false;
        public bool isRecovery = false;
        
        // Board progress tracking
        public int boardProgress = 0;
        
        // FIXED: Monetization anchor - easier level for monetization opportunities
        [Tooltip("If true, this level is easier to provide monetization opportunities")]
        public bool isAnchor = false;
        public bool monetizationAnchor = false;

        // Spawn settings
        public int maxSpawn = 50;
        public int pocketCount = 5;
        public int pocketCapacity = 3;

        // FIXED: Difficulty profile - tied to monetization
        [Range(0.3f, 0.5f)]
        [Tooltip("Target win rate for this level - used for monetization tuning")]
        public float targetWinRate = 0.4f;  // FIXED: Default to 0.3-0.5 range
        
        public float targetFailRate = 0.7f;

        // NEW: Difficulty anchor level - used to create fair monetization offers
        [Tooltip("Anchor level ID for fair difficulty comparison")]
        public int anchorLevelId = -1;
        
        [Tooltip("Difficulty offset from anchor level")]
        public float difficultyOffset = 0f;
    }

    public enum TargetType
    {
        FillSlots,
        ClearLocked,
        DeliverToGate,
        Hybrid
    }

    /// <summary>
    /// Difficulty profile for level tuning
    /// </summary>
    [System.Serializable]
    public class DifficultyProfile
    {
        public float spawnWeightModifier = 1.0f;
        public float conveyorSpeedModifier = 1.0f;
        public float bonusFillRateModifier = 1.0f;
        public float minClusterModifier = 0;
        
        // NEW: Anchor-based difficulty
        [Tooltip("Difficulty anchor - easier levels for monetization")]
        public bool isMonetizationAnchor = false;
        
        [Tooltip("Expected completion rate (0.3-0.5 for new players)")]
        [Range(0.1f, 0.9f)]
        public float expectedWinRate = 0.4f;
    }
}
