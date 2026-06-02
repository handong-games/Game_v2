using System;
using UnityEngine;

namespace Game.Data
{
    public readonly struct ModelKey : IEquatable<ModelKey>
    {
        public ModelKey(Type keyType, int keyValue)
        {
            KeyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
            KeyValue = keyValue;
        }

        public Type KeyType { get; }
        public int KeyValue { get; }

        public bool Equals(ModelKey other)
        {
            return KeyType == other.KeyType && KeyValue == other.KeyValue;
        }

        public override bool Equals(object obj)
        {
            return obj is ModelKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(KeyType, KeyValue);
        }

        public override string ToString()
        {
            return $"{KeyType.Name}:{KeyValue}";
        }
    }

    public abstract class AbstractModel : ScriptableObject
    {
        [SerializeField]
        private string _name;

        public string Name => _name;
        public abstract ModelKey ModelKey { get; }
    }

    public abstract class AbstractModel<TKey> : AbstractModel, IKeyAssignable<TKey>
        where TKey : Enum
    {
        [NonSerialized]
        private TKey _id;

        public TKey Id => _id;
        public override ModelKey ModelKey => new(typeof(TKey), Convert.ToInt32(_id));

        public void SetId(TKey id)
        {
            _id = id;
        }
    }
}
