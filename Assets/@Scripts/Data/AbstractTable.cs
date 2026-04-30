using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Data
{
    public abstract class AbstractTable<TModel, TKey> : ScriptableObject
        where TModel : AbstractModel<TKey>
        where TKey : Enum
    {
        [SerializeField]
        protected List<TModel> _rows = new();

        private void OnEnable()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i] == null)
                    continue;

                _rows[i].SetId((TKey)Enum.ToObject(typeof(TKey), i));
            }
        }

        public TModel Get(int index)
        {
            return _rows[index];
        }

        public List<TModel> GetAll()
        {
            return _rows;
        }
    }
}
