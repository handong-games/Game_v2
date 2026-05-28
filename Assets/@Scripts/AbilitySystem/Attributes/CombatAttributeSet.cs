using Gameplay.GAS;

namespace Game.AbilitySystem.Attributes
{
    public sealed class CombatAttributeSet : AttributeSet
    {
        public static readonly GameplayAttribute CoinCountAttribute =
            GameplayAttribute.Create<CombatAttributeSet>(nameof(CoinCount));
        public static readonly GameplayAttribute CoinHeadsAttribute =
            GameplayAttribute.Create<CombatAttributeSet>(nameof(CoinHeads));
        public static readonly GameplayAttribute CoinTailsAttribute =
            GameplayAttribute.Create<CombatAttributeSet>(nameof(CoinTails));

        [AttributeDefaultValue]
        public GameplayAttributeData CoinCount = new(0f);

        public GameplayAttributeData CoinHeads = new(0f);
        public GameplayAttributeData CoinTails = new(0f);
    }
}
