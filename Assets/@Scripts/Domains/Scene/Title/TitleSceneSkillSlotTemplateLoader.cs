using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Domains.Scene.Title
{
    // Role:
    // Loads the shared skill slot UXML template needed by TitleScene-owned screens.
    public sealed class TitleSceneSkillSlotTemplateLoader : IDisposable
    {
        private const string SkillSlotWidgetAddress = "SkillSlotWidget";

        private AsyncOperationHandle<VisualTreeAsset> _handle;
        private VisualTreeAsset _template;
        private bool _hasHandle;
        private bool _isLoading;
        private bool _disposed;

        public async Awaitable<VisualTreeAsset> Load()
        {
            if (_disposed)
                return null;

            if (_template != null)
                return _template;

            while (_isLoading)
            {
                await Awaitable.NextFrameAsync();
                if (_template != null || _disposed)
                    return _template;
            }

            _isLoading = true;
            try
            {
                _handle = Addressables.LoadAssetAsync<VisualTreeAsset>(SkillSlotWidgetAddress);
                _hasHandle = true;
                _template = await WaitForAddressableLoad(_handle);
                return _template;
            }
            catch
            {
                ReleaseHandle();
                throw;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _template = null;
            ReleaseHandle();
        }

        private void ReleaseHandle()
        {
            if (_hasHandle && _handle.IsValid())
            {
                Addressables.Release(_handle);
            }

            _handle = default;
            _hasHandle = false;
        }

        private static async Awaitable<T> WaitForAddressableLoad<T>(
            AsyncOperationHandle<T> handle)
        {
            while (!handle.IsDone)
            {
                await Awaitable.NextFrameAsync();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw handle.OperationException ?? new InvalidOperationException($"Failed to load {typeof(T).Name}.");

            return handle.Result;
        }
    }
}
