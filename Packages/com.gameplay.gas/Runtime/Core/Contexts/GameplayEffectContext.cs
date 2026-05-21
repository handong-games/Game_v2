namespace Gameplay.GAS
{
    public sealed class GameplayEffectContext
    {
        public GameplayEffectContext(AbilitySystemComponent source, AbilitySystemComponent target)
        {
            Source = source;
            Target = target;
        }

        public AbilitySystemComponent Source { get; }
        public AbilitySystemComponent Target { get; }

        public GameplayEffectContext WithTarget(AbilitySystemComponent target)
        {
            return new GameplayEffectContext(Source, target);
        }
    }
}
