using System;

namespace Gameplay.GAS
{
    public sealed class GameplayAttributeData
    {
        public GameplayAttributeData(float baseValue)
        {
            BaseValue = baseValue;
            CurrentValue = baseValue;
        }

        public event Action<float, float> CurrentValueChanged;

        public float BaseValue { get; private set; }
        public float CurrentValue { get; private set; }

        public void SetBaseValue(float value)
        {
            BaseValue = value;
        }

        public void SetCurrentValue(float value)
        {
            if (CurrentValue.Equals(value))
                return;

            float previousValue = CurrentValue;
            CurrentValue = value;
            CurrentValueChanged?.Invoke(previousValue, CurrentValue);
        }
    }
}
