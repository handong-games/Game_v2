using Domains.Settings;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.Audio
{
    [ManagerDependency(typeof(SaveManager))]
    public sealed class AudioManager : BaseManager<AudioManager>
    {
        private AudioSettingsState _settings;
        private GameObject _audioRoot;
        private AudioSource[] _audioSources = new AudioSource[(int)EAudioPlay.Count];
        
        protected override void OnInit()
        {
            _audioRoot = new GameObject("@AudioManager");
            Object.DontDestroyOnLoad(_audioRoot);
            _audioRoot.AddComponent<AudioManagerBehaviour>();
            _audioRoot.AddComponent<AudioListener>();

            _audioSources[(int)EAudioPlay.BGM] = CreateSource(loop: true);
            _audioSources[(int)EAudioPlay.SFX] = CreateSource(loop: false);
        }

        protected override void OnPostInit()
        {
            _settings = DependencyManager.Instance.Resolve<AudioSettingsState>();
            SetVolume(EAudioVolume.Master, _settings.MasterVolume);
            SetVolume(EAudioVolume.BGM, _settings.BgmVolume);
            SetVolume(EAudioVolume.SFX, _settings.SfxVolume);
        }

        protected override void OnDispose()
        {
            if (_audioRoot != null)
            {
                Object.Destroy(_audioRoot);
                _audioRoot = null;
            }

            _audioSources = new AudioSource[(int)EAudioPlay.Count];
        }
        
        public void Play(EAudioPlay type, AudioClip audio)
        {
            AudioSource source = _audioSources[(int)type];

            if (type == EAudioPlay.BGM)
            {
                source.clip = audio;
                source.Play();
            }
            else if (type == EAudioPlay.SFX)
            {
                source.clip = audio;
                source.PlayOneShot(audio);
            }
        }

        public void SetVolume(EAudioVolume volume, float value)
        {
            if (volume == EAudioVolume.Master)
            {
                _settings.MasterVolume = value;
                ApplyChannelVolumes();
            }
            else if (volume == EAudioVolume.BGM)
            {
                _settings.BgmVolume = value;
                ApplyChannelVolumes();
            }
            else if (volume == EAudioVolume.SFX)
            {
                _settings.SfxVolume = value;
                ApplyChannelVolumes();
            }
        }

        public void SetMuteInBackground(bool enabled)
        {
            _settings.MuteInBackground = enabled;
            if (!enabled)
            {
                AudioListener.pause = false;
            }
        }

        public float GetVolume(EAudioVolume volume)
        {
            float volumeValue = 0f;
            
            if (volume == EAudioVolume.Master)
            {
                volumeValue = _settings.MasterVolume;
            }
            
            if (volume == EAudioVolume.BGM)
            {
                volumeValue = _settings.BgmVolume;
            }
            
            if (volume == EAudioVolume.SFX)
            {
                volumeValue = _settings.SfxVolume;
            }

            return volumeValue;
        }

        public bool GetMuteInBackground()
        {
            return _settings.MuteInBackground;
        }

        public void Stop(EAudioPlay type)
        {
            AudioSource source = _audioSources[(int)type];
            if (source == null)
                return;

            if (type == EAudioPlay.BGM)
            {
                source.Stop();
            }
        }

        private AudioSource CreateSource(bool loop)
        {
            AudioSource source = _audioRoot.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }

        private void ApplyChannelVolumes()
        {
            AudioSource bgmSource = _audioSources[(int)EAudioPlay.BGM];
            if (bgmSource != null)
            {
                bgmSource.volume = _settings.MasterVolume * _settings.BgmVolume;
            }

            AudioSource sfxSource = _audioSources[(int)EAudioPlay.SFX];
            if (sfxSource != null)
            {
                sfxSource.volume = _settings.MasterVolume * _settings.SfxVolume;
            }
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (!_settings.MuteInBackground)
                return;

            AudioListener.pause = pauseStatus;
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!_settings.MuteInBackground)
                return;

            AudioListener.pause = !hasFocus;
        }
    }
}
