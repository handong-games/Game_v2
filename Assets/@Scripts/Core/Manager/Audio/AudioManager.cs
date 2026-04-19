using Domains.Settings;
using Game.Core.Managers.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.Audio
{
    public sealed class AudioManager : BaseManager<AudioManager>
    {
        private AudioSaveData _saveData;
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

            _saveData = SettingManager.Instance.SaveData.AudioSaveData;
            SetVolume(EAudioVolume.Master, _saveData.masterVolume);
            SetVolume(EAudioVolume.BGM, _saveData.bgmVolume);
            SetVolume(EAudioVolume.SFX, _saveData.sfxVolume);
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
                _saveData.masterVolume = value;
                ApplyChannelVolumes();
            }
            else if (volume == EAudioVolume.BGM)
            {
                _saveData.bgmVolume = value;
                ApplyChannelVolumes();
            }
            else if (volume == EAudioVolume.SFX)
            {
                _saveData.sfxVolume = value;
                ApplyChannelVolumes();
            }
        }

        public void SetMuteInBackground(bool enabled)
        {
            _saveData.muteInBackground = enabled;
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
                volumeValue = _saveData.masterVolume;
            }
            
            if (volume == EAudioVolume.BGM)
            {
                volumeValue = _saveData.bgmVolume;
            }
            
            if (volume == EAudioVolume.SFX)
            {
                volumeValue = _saveData.sfxVolume;
            }

            return volumeValue;
        }

        public bool GetMuteInBackground()
        {
            return _saveData.muteInBackground;
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
                bgmSource.volume = _saveData.masterVolume * _saveData.bgmVolume;
            }

            AudioSource sfxSource = _audioSources[(int)EAudioPlay.SFX];
            if (sfxSource != null)
            {
                sfxSource.volume = _saveData.masterVolume * _saveData.sfxVolume;
            }
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (!_saveData.muteInBackground)
                return;

            AudioListener.pause = pauseStatus;
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!_saveData.muteInBackground)
                return;

            AudioListener.pause = !hasFocus;
        }
    }
}
