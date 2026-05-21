using System;

namespace Gameplay.GAS
{
    public readonly struct GameplayTag : IEquatable<GameplayTag>
    {
        internal GameplayTag(int id)
        {
            Id = id;
        }

        public GameplayTag(string name)
        {
            Id = GameplayTagRegistry.RequestId(name);
        }

        internal int Id { get; }
        public bool IsValid => Id > 0;

        public static GameplayTag Request(string name)
        {
            return GameplayTagRegistry.Request(name);
        }

        public bool MatchesTag(GameplayTag other)
        {
            return GameplayTagRegistry.MatchesTag(this, other);
        }

        public bool MatchesTagExact(GameplayTag other)
        {
            return Equals(other);
        }

        public GameplayTag RequestDirectParent()
        {
            return GameplayTagRegistry.RequestDirectParent(this);
        }

        public bool Equals(GameplayTag other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return GameplayTagRegistry.GetName(this);
        }

        public static bool operator ==(GameplayTag left, GameplayTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GameplayTag left, GameplayTag right)
        {
            return !left.Equals(right);
        }
    }
}
