using System;

namespace Gameplay.GAS
{
    public readonly struct GameplayAttribute : IEquatable<GameplayAttribute>
    {
        public GameplayAttribute(string name)
        {
            Name = name?.Trim() ?? string.Empty;
        }

        public string Name { get; }
        public bool IsValid => !string.IsNullOrEmpty(Name);

        public bool Equals(GameplayAttribute other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayAttribute other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Name ?? string.Empty);
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==(GameplayAttribute left, GameplayAttribute right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameplayAttribute left, GameplayAttribute right)
        {
            return !left.Equals(right);
        }
    }
}
