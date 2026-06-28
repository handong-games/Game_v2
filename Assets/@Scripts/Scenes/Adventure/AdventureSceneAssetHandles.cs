using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scenes.Adventure
{
    // Role:
    // Owns Addressables handles returned by AbstractTable.LoadAsync for one AdventureScene lifetime.
    // It releases those handles when AdventureSceneScope is disposed.
    public sealed class AdventureSceneAssetHandles : IDisposable
    {
        private readonly List<AsyncOperationHandle> _handles = new();

        public void Add(AsyncOperationHandle handle)
        {
            if (!handle.IsValid())
                throw new ArgumentException("Addressables handle is invalid.", nameof(handle));

            _handles.Add(handle);
        }

        public void Add<T>(AsyncOperationHandle<T> handle)
        {
            Add((AsyncOperationHandle)handle);
        }

        public void Dispose()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                AsyncOperationHandle handle = _handles[i];
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _handles.Clear();
        }
    }
}
