using UnityEngine;

namespace Game.Core.Managers.Audio
{
    public sealed class AudioManagerBehaviour : MonoBehaviour
    {
        private void OnApplicationPause(bool pauseStatus)
        {
            AudioManager.Instance.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            AudioManager.Instance.OnApplicationFocus(hasFocus);
        }
    }
}
