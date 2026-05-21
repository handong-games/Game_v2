namespace Gameplay.GAS
{
    public sealed class GameplayEffectCue
    {
        public GameplayEffectCue(float minLevel = 0f, float maxLevel = 0f)
        {
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }

        public GameplayTagContainer GameplayCueTags { get; } = new();
        public float MinLevel { get; set; }
        public float MaxLevel { get; set; }

        public float NormalizeLevel(float level)
        {
            float range = MaxLevel - MinLevel;
            if (range <= 0f)
                return 1f;

            float value = (level - MinLevel) / range;
            if (value < 0f)
                return 0f;

            return value > 1f ? 1f : value;
        }
    }
}
