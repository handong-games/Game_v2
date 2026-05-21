using System;

namespace Gameplay.GAS
{
    public readonly struct ActiveGameplayEffectHandle : IEquatable<ActiveGameplayEffectHandle>
    {
        public static readonly ActiveGameplayEffectHandle Invalid = new(0);

        public ActiveGameplayEffectHandle(int value)
        {
            Value = value;
        }

        public int Value { get; }
        public bool IsValid => Value > 0;

        public bool Equals(ActiveGameplayEffectHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ActiveGameplayEffectHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}
