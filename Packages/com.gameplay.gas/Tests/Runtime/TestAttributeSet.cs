namespace Gameplay.GAS.Tests
{
    public sealed class TestAttributeSet : AttributeSet
    {
        public static readonly GameplayAttribute HealthAttribute =
            GameplayAttribute.Create<TestAttributeSet>(nameof(Health));
        public static readonly GameplayAttribute AttackAttribute =
            GameplayAttribute.Create<TestAttributeSet>(nameof(Attack));
        public static readonly GameplayAttribute EnergyAttribute =
            GameplayAttribute.Create<TestAttributeSet>(nameof(Energy));

        public GameplayAttributeData Health = new(0f);
        public GameplayAttributeData Attack = new(0f);
        public GameplayAttributeData Energy = new(0f);
    }
}
