namespace Gameplay.GAS
{
    public sealed class GameplayEventData
    {
        public GameplayEventData(GameplayTag eventTag)
        {
            EventTag = eventTag;
        }

        public GameplayTag EventTag { get; }
        public AbilitySystemComponent Instigator { get; set; }
        public AbilitySystemComponent Target { get; set; }
        public AbilitySystemComponent ResolvedTarget => Target ?? Instigator;
        public object OptionalObject { get; set; }
        public float EventMagnitude { get; set; }

        public bool TryGetOptionalObject<T>(out T optionalObject) where T : class
        {
            optionalObject = OptionalObject as T;
            return optionalObject != null;
        }
    }
}
