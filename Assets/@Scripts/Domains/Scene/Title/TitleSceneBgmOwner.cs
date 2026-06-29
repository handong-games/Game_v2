using System;
using Domains.Settings;
using Game.Core.Managers.Audio;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Domains.Scene.Title
{
    public sealed class TitleSceneBgmOwner : IDisposable
    {
        private const string Address = "TitleMenuBgm";

        private readonly AudioManager _audioManager;
        private AsyncOperationHandle<AudioClip> _handle;
        private bool _hasHandle;
        private bool _disposed;

        public TitleSceneBgmOwner(AudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public async Awaitable LoadAndPlay()
        {
            if (_disposed || _hasHandle)
                return;

            _handle = Addressables.LoadAssetAsync<AudioClip>(Address);
            _hasHandle = true;
            AudioClip clip;
            try
            {
                clip = await WaitForAddressableLoad(_handle);
            }
            catch
            {
                ReleaseHandle();
                throw;
            }

            if (_disposed)
            {
                ReleaseHandle();
                return;
            }

            if (clip == null)
            {
                ReleaseHandle();
                return;
            }

            _audioManager.Play(EAudioPlay.BGM, clip);
        }

        public void Dispose()
        {
            _disposed = true;
            _audioManager.Stop(EAudioPlay.BGM);

            if (!_hasHandle)
                return;

            ReleaseHandle();
        }

        private void ReleaseHandle()
        {
            if (_handle.IsValid())
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
