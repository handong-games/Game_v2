namespace Gameplay.GAS
{
    public sealed class GameplayEffectContext
    {
        public GameplayEffectContext(
            AbilitySystemComponent source,
            AbilitySystemComponent target,
            object sourceObject = null)
        {
            Source = source;
            Target = target;
            SourceObject = sourceObject;
        }

        public AbilitySystemComponent Source { get; }
        public AbilitySystemComponent Target { get; }
        public object SourceObject { get; }

        public GameplayEffectContext WithTarget(AbilitySystemComponent target)
        {
            return new GameplayEffectContext(Source, target, SourceObject);
        }

        public bool TryGetSourceObject<T>(out T sourceObject)
        {
            if (SourceObject is T typedSourceObject)
            {
                sourceObject = typedSourceObject;
                return true;
            }

            sourceObject = default;
            return false;
        }
    }
}
