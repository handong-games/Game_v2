namespace Gameplay.GAS
{
    public readonly struct GameplayModifierEvaluatedData
    {
        public GameplayModifierEvaluatedData(
            GameplayAttribute attribute,
            GameplayModifierOperation operation,
            float magnitude)
        {
            Attribute = attribute;
            Operation = operation;
            Magnitude = magnitude;
        }

        public GameplayAttribute Attribute { get; }
        public GameplayModifierOperation Operation { get; }
        public float Magnitude { get; }
    }
}
