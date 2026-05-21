using System;

namespace Gameplay.GAS
{
    public readonly struct GameplayEffectAttributeCaptureDefinition :
        IEquatable<GameplayEffectAttributeCaptureDefinition>
    {
        public GameplayEffectAttributeCaptureDefinition(
            GameplayAttribute attribute,
            GameplayEffectAttributeCaptureSource source,
            bool snapshot)
        {
            Attribute = attribute;
            Source = source;
            Snapshot = snapshot;
        }

        public GameplayAttribute Attribute { get; }
        public GameplayEffectAttributeCaptureSource Source { get; }
        public bool Snapshot { get; }
        public bool IsValid => Attribute.IsValid;

        public bool Equals(GameplayEffectAttributeCaptureDefinition other)
        {
            return Attribute.Equals(other.Attribute) &&
                   Source == other.Source &&
                   Snapshot == other.Snapshot;
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayEffectAttributeCaptureDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Attribute.GetHashCode();
                hashCode = (hashCode * 397) ^ Source.GetHashCode();
                hashCode = (hashCode * 397) ^ Snapshot.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(
            GameplayEffectAttributeCaptureDefinition left,
            GameplayEffectAttributeCaptureDefinition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            GameplayEffectAttributeCaptureDefinition left,
            GameplayEffectAttributeCaptureDefinition right)
        {
            return !left.Equals(right);
        }
    }
}
