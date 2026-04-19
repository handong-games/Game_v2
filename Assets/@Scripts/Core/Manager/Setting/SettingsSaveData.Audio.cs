using System;
using UnityEngine;

namespace Domains.Settings
{
    [Serializable]
    public class AudioSaveData
    {
        public float masterVolume = 0.5f;
        public float bgmVolume = 1.0f;
        public float sfxVolume = 1.0f;
        public bool muteInBackground = true;
    }
    
    public sealed partial class SettingsSaveData
    {
        [SerializeField]
        private AudioSaveData _audioSaveData = new AudioSaveData();
        public  AudioSaveData AudioSaveData => _audioSaveData;
        
        private void NormalizeAudio()
        {
            _audioSaveData.masterVolume = Mathf.Clamp01(_audioSaveData.masterVolume);
            _audioSaveData.bgmVolume = Mathf.Clamp01(_audioSaveData.bgmVolume);
            _audioSaveData.sfxVolume = Mathf.Clamp01(_audioSaveData.sfxVolume);
        }
    }
}
