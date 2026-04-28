using UnityEngine;

namespace Game.Data
{
    public abstract class AbstractModel<TKey> : ScriptableObject, IKeyAssignable<TKey> where TKey : global::System.Enum
    {
        [global::System.NonSerialized]
        private TKey _key;

        public TKey Key => _key;
        public string Name;

        public void SetKey(TKey key)
        {
            _key = key;
        }
    }
}
