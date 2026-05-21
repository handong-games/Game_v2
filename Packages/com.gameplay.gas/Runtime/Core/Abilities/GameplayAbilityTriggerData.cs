namespace Gameplay.GAS
{
    public readonly struct GameplayAbilityTriggerData
    {
        public GameplayAbilityTriggerData(
            GameplayAbilityTriggerSource triggerSource,
            GameplayTag triggerTag)
        {
            TriggerSource = triggerSource;
            TriggerTag = triggerTag;
        }

        public GameplayAbilityTriggerSource TriggerSource { get; }
        public GameplayTag TriggerTag { get; }
        public bool IsValid => TriggerTag.IsValid;
    }
}
