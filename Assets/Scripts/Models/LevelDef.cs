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
        public bool isAnchor = false;

        // Spawn settings
        public int maxSpawn = 50;
        public int pocketCount = 5;
        public int pocketCapacity = 3;

        // Difficulty profile
        public float targetWinRate = 0.3f;
        public float targetFailRate = 0.7f;

        // Monetization
        public bool monetizationAnchor = false;
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
    }
}
