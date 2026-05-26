using System;
using System.Reflection;

namespace Gameplay.GAS
{
    public readonly struct GameplayAttribute : IEquatable<GameplayAttribute>
    {
        private GameplayAttribute(
            Type attributeSetType,
            string fieldName,
            FieldInfo fieldInfo)
        {
            AttributeSetType = attributeSetType;
            FieldName = fieldName?.Trim() ?? string.Empty;
            FieldInfo = fieldInfo;
        }

        public GameplayAttribute(string fieldName)
            : this(null, fieldName, null)
        {
        }

        public Type AttributeSetType { get; }
        public string FieldName { get; }
        internal FieldInfo FieldInfo { get; }

        public bool IsValid => !string.IsNullOrEmpty(FieldName);

        public static GameplayAttribute Create<TAttributeSet>(string fieldName)
            where TAttributeSet : AttributeSet
        {
            return Create(typeof(TAttributeSet), fieldName);
        }

        public static GameplayAttribute Create(Type attributeSetType, string fieldName)
        {
            if (attributeSetType == null)
                return default;

            if (!typeof(AttributeSet).IsAssignableFrom(attributeSetType))
                return default;

            if (string.IsNullOrWhiteSpace(fieldName))
                return default;

            FieldInfo fieldInfo = attributeSetType.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return new GameplayAttribute(attributeSetType, fieldName, fieldInfo);
        }

        public bool Equals(GameplayAttribute other)
        {
            return AttributeSetType == other.AttributeSetType &&
                   string.Equals(FieldName, other.FieldName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is GameplayAttribute other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AttributeSetType, StringComparer.Ordinal.GetHashCode(FieldName ?? string.Empty));
        }

        public override string ToString()
        {
            return AttributeSetType == null
                ? FieldName
                : $"{AttributeSetType.Name}.{FieldName}";
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
