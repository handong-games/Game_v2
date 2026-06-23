using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Data
{
    public abstract class AbstractTable<TModel, TKey> : ScriptableObject
        where TModel : AbstractModel, IKeyAssignable<TKey>
        where TKey : Enum
    {
        [SerializeField]
        protected List<AssetReferenceT<TModel>> _rows = new();

        private readonly Dictionary<int, AsyncOperationHandle<TModel>> _ownedHandles = new();

        // Compatibility path for small, bounded datasets only.
        // This can synchronously load Addressables through LoadSync and block the main thread.
        // Prefer LoadAsync for scene preload, gameplay flow, combat runtime, and large tables.
        public TModel Get(int index)
        {
            if (_ownedHandles.TryGetValue(index, out AsyncOperationHandle<TModel> cachedHandle) &&
                cachedHandle.IsValid())
            {
                return cachedHandle.Result;
            }

            TKey key = (TKey)Enum.ToObject(typeof(TKey), index);
            AsyncOperationHandle<TModel> handle = LoadSync(key, out TModel model);
            _ownedHandles[index] = handle;
            return model;
        }

        public TModel Get(TKey key)
        {
            return Get(Convert.ToInt32(key));
        }

        // Preferred path for runtime loading. The caller owns the returned handle and must release it.
        public AsyncOperationHandle<TModel> LoadAsync(TKey key)
        {
            int index = Convert.ToInt32(key);
            AssetReferenceT<TModel> reference = GetReference(index, key);
            AsyncOperationHandle<TModel> handle = Addressables.LoadAssetAsync<TModel>(reference);
            handle.Completed += operation =>
            {
                if (operation.Result != null)
                {
                    operation.Result.SetId(key);
                }
            };

            return handle;
        }

        // Compatibility path for editor checks, tests, bootstrap code, and deliberately small datasets.
        // Do not use during scene transitions, animation flow, combat runtime, or repeated interactions.
        public AsyncOperationHandle<TModel> LoadSync(TKey key, out TModel model)
        {
            AsyncOperationHandle<TModel> handle = LoadAsync(key);
            model = handle.WaitForCompletion();
            if (model != null)
            {
                model.SetId(key);
            }

            return handle;
        }

        // Compatibility path. This may synchronously load every row in this table.
        // Keep usage limited to small lists such as character selection.
        public List<TModel> GetAll()
        {
            List<TModel> models = new(_rows.Count);
            for (int i = 0; i < _rows.Count; i++)
            {
                models.Add(Get(i));
            }

            return models;
        }

        public void ReleaseLoadedAssets()
        {
            foreach (AsyncOperationHandle<TModel> handle in _ownedHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _ownedHandles.Clear();
        }

        protected AssetReferenceT<TModel> GetReference(int index, TKey key)
        {
            if (index < 0 || index >= _rows.Count)
                throw new IndexOutOfRangeException($"{typeof(TModel).Name} reference is missing: {key}");

            AssetReferenceT<TModel> reference = _rows[index];
            if (reference == null || !reference.RuntimeKeyIsValid())
                throw new InvalidOperationException($"{typeof(TModel).Name} reference is invalid: {key}");

            return reference;
        }
    }
}
