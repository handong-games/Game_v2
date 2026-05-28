namespace Gameplay.GAS
{
    public sealed class GameplayModifierSpec
    {
        public GameplayModifierSpec(GameplayModifier modifier, float evaluatedMagnitude)
        {
            Modifier = modifier;
            EvaluatedMagnitude = evaluatedMagnitude;
        }

        public GameplayModifier Modifier { get; }
        public float EvaluatedMagnitude { get; }

        public GameplayModifierEvaluatedData EvaluatedData =>
            new(Modifier.Attribute, Modifier.Operation, EvaluatedMagnitude);
    }
}
