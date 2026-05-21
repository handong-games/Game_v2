namespace Gameplay.GAS
{
    public sealed class GameplayCueEventData
    {
        public GameplayCueEventData(
            AbilitySystemComponent target,
            GameplayTag cueTag,
            GameplayCueEvent eventType,
            GameplayCueParameters parameters)
        {
            Target = target;
            CueTag = cueTag;
            EventType = eventType;
            Parameters = parameters;
        }

        public AbilitySystemComponent Target { get; }
        public GameplayTag CueTag { get; }
        public GameplayCueEvent EventType { get; }
        public GameplayCueParameters Parameters { get; }
    }
}
