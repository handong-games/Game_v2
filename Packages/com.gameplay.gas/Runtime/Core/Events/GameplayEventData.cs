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
        public float EventMagnitude { get; set; }
    }
}
