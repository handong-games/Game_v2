using System;
using Game.Core.Define;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public sealed class SettingsSave : ISave
    {
        public const int CurrentVersion = 1;

        public int SaveVersion = CurrentVersion;

        public float MasterVolume = 0.5f;
        public float BgmVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public bool MuteInBackground = true;

        public string LanguageCode = "en-US";

        public bool Fullscreen = true;
        public int WindowedWidth = Screen.currentResolution.width;
        public int WindowedHeight = Screen.currentResolution.height;
        public int TargetDisplayIndex = 0;
        public int WindowPositionX = -1;
        public int WindowPositionY = -1;
        public EDisplayAspect Aspect = EDisplayAspect.Auto;
    }
}
