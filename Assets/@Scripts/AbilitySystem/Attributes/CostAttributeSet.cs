using Game.AbilitySystem;
using Gameplay.GAS;

namespace Game.AbilitySystem.Attributes
{
    public sealed class CostAttributeSet : AttributeSet
    {
        public static readonly GameplayAttribute CoinCountAttribute =
            GameplayAttribute.Create<CostAttributeSet>(nameof(CoinCount));
        public static readonly GameplayAttribute CoinHeadsAttribute =
            GameplayAttribute.Create<CostAttributeSet>(nameof(CoinHeads));
        public static readonly GameplayAttribute CoinTailsAttribute =
            GameplayAttribute.Create<CostAttributeSet>(nameof(CoinTails));

        [AttributeDefaultValue]
        public GameplayAttributeData CoinCount = new(0f);

        public GameplayAttributeData CoinHeads = new(0f);
        public GameplayAttributeData CoinTails = new(0f);

        public void RestoreCoinsToBase()
        {
            CoinHeads.SetCurrentValue(CoinHeads.BaseValue);
            CoinTails.SetCurrentValue(CoinTails.BaseValue);
        }

        protected override void OnInitialized()
        {
            CoinHeads.CurrentValueChanged += OnCoinValueChanged;
            CoinTails.CurrentValueChanged += OnCoinValueChanged;
        }

        private void OnCoinValueChanged(float oldValue, float newValue)
        {
            AbilitySystem?.HandleGameplayEvent(
                new GameplayEventData(AbilityGameplayTags.EventCostCoinChanged)
                {
                    Instigator = AbilitySystem,
                });
        }
    }
}
