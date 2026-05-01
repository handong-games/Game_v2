using Game.Core.Managers.Dependency;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    [Dependency]
    public sealed class AudioSettingsState : ISave<AudioSettingsSave>
    {
        public float MasterVolume { get; set; } = 0.5f;
        public float BgmVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 1.0f;
        public bool MuteInBackground { get; set; } = true;

        public void LoadFrom(AudioSettingsSave save)
        {
            MasterVolume = Mathf.Clamp01(save.MasterVolume);
            BgmVolume = Mathf.Clamp01(save.BgmVolume);
            SfxVolume = Mathf.Clamp01(save.SfxVolume);
            MuteInBackground = save.MuteInBackground;
        }

        public AudioSettingsSave ToSave()
        {
            return new AudioSettingsSave
            {
                MasterVolume = MasterVolume,
                BgmVolume = BgmVolume,
                SfxVolume = SfxVolume,
                MuteInBackground = MuteInBackground
            };
        }
    }
}
