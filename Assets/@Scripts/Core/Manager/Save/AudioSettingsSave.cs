using System;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public sealed class AudioSettingsSave : SaveData
    {
        public const int CurrentVersion = 1;

        public float MasterVolume = 0.5f;
        public float BgmVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public bool MuteInBackground = true;

        public AudioSettingsSave() : base(CurrentVersion)
        {
        }
    }
}
