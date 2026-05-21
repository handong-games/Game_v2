namespace Gameplay.GAS
{
    public sealed class GameplayCueParameters
    {
        public GameplayCueParameters(
            GameplayEffectContext context = null,
            float normalizedMagnitude = 0f,
            float rawMagnitude = 0f,
            int gameplayEffectLevel = 1)
        {
            Context = context;
            NormalizedMagnitude = normalizedMagnitude;
            RawMagnitude = rawMagnitude;
            GameplayEffectLevel = gameplayEffectLevel;
        }

        public GameplayEffectContext Context { get; }
        public float NormalizedMagnitude { get; }
        public float RawMagnitude { get; }
        public int GameplayEffectLevel { get; }
    }
}
