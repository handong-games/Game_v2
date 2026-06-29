using System;
using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Domains.Scene.Title
{
    // Role:
    // Loads shared card face templates needed by TitleScene-owned screens.
    public sealed class TitleSceneCardFaceTemplateLoader : IDisposable
    {
        private const string PortraitFaceWidgetAddress = "PortraitCardFaceWidget";
        private const string LockedFaceWidgetAddress = "LockedCardFaceWidget";

        private AsyncOperationHandle<VisualTreeAsset> _portraitHandle;
        private AsyncOperationHandle<VisualTreeAsset> _lockedHandle;
        private CardFaceWidgetTemplates _templates;
        private bool _isLoading;

        public async Awaitable<CardFaceWidgetTemplates> Load()
        {
            if (_templates != null)
                return _templates;

            while (_isLoading)
            {
                await Awaitable.NextFrameAsync();
                if (_templates != null)
                    return _templates;
            }

            _isLoading = true;
            try
            {
                _portraitHandle = Addressables.LoadAssetAsync<VisualTreeAsset>(PortraitFaceWidgetAddress);
                _lockedHandle = Addressables.LoadAssetAsync<VisualTreeAsset>(LockedFaceWidgetAddress);

                VisualTreeAsset portrait = await WaitForAddressableLoad(_portraitHandle);
                VisualTreeAsset locked = await WaitForAddressableLoad(_lockedHandle);

                _templates = new CardFaceWidgetTemplates(portrait, locked);
                return _templates;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void Dispose()
        {
            Release(_lockedHandle);
            Release(_portraitHandle);
            _templates = null;
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

        private static void Release(AsyncOperationHandle handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}
