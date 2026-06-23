using UnityEngine;

namespace Game.Core.Managers.Audio
{
    public sealed class AudioManagerBehaviour : MonoBehaviour
    {
        private AudioManager _audioManager;

        public void Initialize(AudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _audioManager?.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _audioManager?.OnApplicationFocus(hasFocus);
        }
    }
}
