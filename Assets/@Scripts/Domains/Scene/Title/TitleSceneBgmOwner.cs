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

        public TitleSceneBgmOwner(AudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public void LoadAndPlay()
        {
            if (_hasHandle)
                return;

            _handle = Addressables.LoadAssetAsync<AudioClip>(Address);
            AudioClip clip = _handle.WaitForCompletion();

            if (clip == null)
            {
                Addressables.Release(_handle);
                _handle = default;
                return;
            }

            _hasHandle = true;
            _audioManager.Play(EAudioPlay.BGM, clip);
        }

        public void Dispose()
        {
            _audioManager.Stop(EAudioPlay.BGM);

            if (!_hasHandle)
                return;

            Addressables.Release(_handle);
            _handle = default;
            _hasHandle = false;
        }
    }
}
