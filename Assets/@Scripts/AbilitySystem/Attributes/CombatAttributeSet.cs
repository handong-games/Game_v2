using Gameplay.GAS;

namespace Game.AbilitySystem.Attributes
{
    public sealed class CombatAttributeSet : AttributeSet
    {
        public static readonly GameplayAttribute PhysicalAttackAttribute =
            GameplayAttribute.Create<CombatAttributeSet>(nameof(PhysicalAttack));

        [AttributeDefaultValue]
        public GameplayAttributeData PhysicalAttack = new(0f);
    }
}
