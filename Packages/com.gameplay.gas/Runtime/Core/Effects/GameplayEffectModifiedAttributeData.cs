namespace Gameplay.GAS
{
    public readonly struct GameplayEffectModifiedAttributeData
    {
        public GameplayEffectModifiedAttributeData(GameplayAttribute attribute, float totalMagnitude)
        {
            Attribute = attribute;
            TotalMagnitude = totalMagnitude;
        }

        public GameplayAttribute Attribute { get; }
        public float TotalMagnitude { get; }
    }
}
