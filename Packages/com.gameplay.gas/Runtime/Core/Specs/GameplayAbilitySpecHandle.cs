using System;

namespace Gameplay.GAS
{
    public readonly struct GameplayAbilitySpecHandle : IEquatable<GameplayAbilitySpecHandle>
    {
        public static readonly GameplayAbilitySpecHandle Invalid = new(0);

        public GameplayAbilitySpecHandle(int value)
        {
            Value = value;
        }

        public int Value { get; }
        public bool IsValid => Value > 0;

        public bool Equals(GameplayAbilitySpecHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayAbilitySpecHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(GameplayAbilitySpecHandle left, GameplayAbilitySpecHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameplayAbilitySpecHandle left, GameplayAbilitySpecHandle right)
        {
            return !left.Equals(right);
        }
    }
}
