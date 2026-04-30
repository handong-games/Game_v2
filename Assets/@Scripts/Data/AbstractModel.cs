using UnityEngine;

namespace Game.Data
{
    public abstract class AbstractModel<TKey> : ScriptableObject, IKeyAssignable<TKey> where TKey : global::System.Enum
    {
        [global::System.NonSerialized]
        private TKey _id;

        [SerializeField]
        private string _name;

        public TKey Id => _id;
        public string Name => _name;

        public void SetId(TKey id)
        {
            _id = id;
        }
    }
}
