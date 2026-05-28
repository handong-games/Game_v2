namespace Gameplay.GAS
{
    internal sealed class GameplayAttributeAggregator
    {
        private float _additiveMagnitude;
        private float _multiplicativeMagnitude = 1f;
        private float _overrideMagnitude;
        private bool _hasOverride;

        public void AddModifier(GameplayModifier modifier, float magnitude, int stackCount = 1)
        {
            AddModifier(modifier.Operation, magnitude, stackCount);
        }

        public void AddModifier(
            GameplayModifierOperation operation,
            float magnitude,
            int stackCount = 1)
        {
            for (int i = 0; i < stackCount; i++)
            {
                AddModifierOnce(operation, magnitude);
            }
        }

        private void AddModifierOnce(GameplayModifierOperation operation, float magnitude)
        {
            switch (operation)
            {
                case GameplayModifierOperation.Add:
                    _additiveMagnitude += magnitude;
                    break;
                case GameplayModifierOperation.Multiply:
                    _multiplicativeMagnitude *= magnitude;
                    break;
                case GameplayModifierOperation.Override:
                    _overrideMagnitude = magnitude;
                    _hasOverride = true;
                    break;
            }
        }

        public float Evaluate(float baseValue)
        {
            if (_hasOverride)
                return _overrideMagnitude;

            return (baseValue + _additiveMagnitude) * _multiplicativeMagnitude;
        }
    }
}
