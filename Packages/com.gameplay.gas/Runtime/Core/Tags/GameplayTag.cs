using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public struct GameplayTag : IEquatable<GameplayTag>
    {
        [SerializeField]
        private string _path;

        // Runtime cache only. Do not serialize or persist this value.
        [NonSerialized]
        private int _runtimeId;

        internal GameplayTag(string path, int runtimeId)
        {
            _path = path;
            _runtimeId = runtimeId;
        }

        public string Path => _path;
        public bool IsValid => !string.IsNullOrWhiteSpace(_path);
        internal int RuntimeId => _runtimeId;

        public static GameplayTag Define(string path)
        {
            return GameplayTagRegistry.Define(path);
        }

        public bool MatchesTag(GameplayTag other)
        {
            return GameplayTagRegistry.MatchesTag(this, other);
        }

        public bool MatchesTagExact(GameplayTag other)
        {
            return Equals(other);
        }

        public GameplayTag GetDirectParent()
        {
            return GameplayTagRegistry.GetDirectParent(this);
        }

        internal GameplayTag WithRuntimeId(int runtimeId)
        {
            return new GameplayTag(_path, runtimeId);
        }

        public bool Equals(GameplayTag other)
        {
            int leftRuntimeId = GameplayTagRegistry.EnsureRuntimeId(ref this);
            int rightRuntimeId = GameplayTagRegistry.EnsureRuntimeId(ref other);

            if (leftRuntimeId > 0 && rightRuntimeId > 0)
                return leftRuntimeId == rightRuntimeId;

            return string.Equals(_path, other._path, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayTag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_path ?? string.Empty);
        }

        public override string ToString()
        {
            return _path ?? string.Empty;
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
