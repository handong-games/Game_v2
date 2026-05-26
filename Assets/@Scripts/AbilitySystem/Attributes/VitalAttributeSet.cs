using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Attributes
{
    public sealed class VitalAttributeSet : AttributeSet
    {
        public static readonly GameplayAttribute HealthAttribute =
            GameplayAttribute.Create<VitalAttributeSet>(nameof(Health));
        public static readonly GameplayAttribute MaxHealthAttribute =
            GameplayAttribute.Create<VitalAttributeSet>(nameof(MaxHealth));
        public static readonly GameplayAttribute PreviewHealthAttribute =
            GameplayAttribute.Create<VitalAttributeSet>(nameof(PreviewHealth));
        public static readonly GameplayAttribute IncomingDamageAttribute =
            GameplayAttribute.Create<VitalAttributeSet>(nameof(IncomingDamage));
        public static readonly GameplayAttribute IncomingPreviewDamageAttribute =
            GameplayAttribute.Create<VitalAttributeSet>(nameof(IncomingPreviewDamage));
    
        [AttributeDefaultValue]
        public GameplayAttributeData Health = new(0f);

        [AttributeDefaultValue]
        public GameplayAttributeData MaxHealth = new(1f);

        public GameplayAttributeData PreviewHealth = new(0f);
        public GameplayAttributeData IncomingDamage = new(0f);
        public GameplayAttributeData IncomingPreviewDamage = new(0f);

        public override void PostGameplayEffectExecute(GameplayEffectModCallbackData data)
        {
            GameplayModifierEvaluatedData evaluatedData = data.EvaluatedData;

            if (evaluatedData.Attribute == IncomingDamageAttribute)
            {
                float nextHealth = ClampHealth(Health.CurrentValue - IncomingDamage.CurrentValue);
                Health.SetBaseValue(nextHealth);
                Health.SetCurrentValue(nextHealth);
                IncomingDamage.SetBaseValue(0f);
                IncomingDamage.SetCurrentValue(0f);
                return;
            }

            if (evaluatedData.Attribute == IncomingPreviewDamageAttribute)
            {
                float nextPreviewHealth = ClampHealth(PreviewHealth.CurrentValue - IncomingPreviewDamage.CurrentValue);
                PreviewHealth.SetBaseValue(nextPreviewHealth);
                PreviewHealth.SetCurrentValue(nextPreviewHealth);
                IncomingPreviewDamage.SetBaseValue(0f);
                IncomingPreviewDamage.SetCurrentValue(0f);
            }
        }

        private float ClampHealth(float value)
        {
            float maxHealth = Mathf.Max(1f, MaxHealth.CurrentValue);
            return Mathf.Clamp(value, 0f, maxHealth);
        }
    }
}
