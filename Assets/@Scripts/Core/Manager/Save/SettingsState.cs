using Game.Core.Define;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Game.Core.Managers.Save
{
    public sealed class SettingsState : IState<SettingsSave>
    {
        public float MasterVolume { get; set; } = 0.5f;
        public float BgmVolume { get; set; } = 1.0f;
        public float SfxVolume { get; set; } = 1.0f;
        public bool MuteInBackground { get; set; } = true;
        public string LanguageCode { get; set; } = "en-US";

        public bool Fullscreen { get; set; } = true;
        public int WindowedWidth { get; set; } = Screen.currentResolution.width;
        public int WindowedHeight { get; set; } = Screen.currentResolution.height;
        public int TargetDisplayIndex { get; set; }
        public int WindowPositionX { get; set; } = -1;
        public int WindowPositionY { get; set; } = -1;
        public EDisplayAspect Aspect { get; set; } = EDisplayAspect.Auto;

        public static SettingsState FromSave(SettingsSave save)
        {
            SettingsState state = new SettingsState
            {
                MasterVolume = Mathf.Clamp01(save.MasterVolume),
                BgmVolume = Mathf.Clamp01(save.BgmVolume),
                SfxVolume = Mathf.Clamp01(save.SfxVolume),
                MuteInBackground = save.MuteInBackground,
                LanguageCode = save.LanguageCode,
                Fullscreen = save.Fullscreen,
                WindowedWidth = save.WindowedWidth,
                WindowedHeight = save.WindowedHeight,
                TargetDisplayIndex = Mathf.Max(0, save.TargetDisplayIndex),
                WindowPositionX = save.WindowPositionX,
                WindowPositionY = save.WindowPositionY,
                Aspect = save.Aspect
            };

            if (state.WindowedWidth <= 0 || state.WindowedHeight <= 0)
            {
                state.WindowedWidth = Screen.currentResolution.width;
                state.WindowedHeight = Screen.currentResolution.height;
            }

            if (state.WindowPositionX < 0 || state.WindowPositionY < 0)
            {
                state.WindowPositionX = -1;
                state.WindowPositionY = -1;
            }

            UnityEngine.Localization.Locale locale = LocalizationSettings.AvailableLocales.GetLocale(state.LanguageCode);
            if (locale == null)
            {
                locale = new SystemLocaleSelector().GetStartupLocale(LocalizationSettings.AvailableLocales)
                    ?? LocalizationSettings.AvailableLocales.GetLocale("en-US");

                if (locale != null)
                {
                    state.LanguageCode = locale.Identifier.Code;
                }
            }

            return state;
        }

        public SettingsSave ToSave()
        {
            return new SettingsSave
            {
                SaveVersion = SettingsSave.CurrentVersion,
                MasterVolume = MasterVolume,
                BgmVolume = BgmVolume,
                SfxVolume = SfxVolume,
                MuteInBackground = MuteInBackground,
                LanguageCode = LanguageCode,
                Fullscreen = Fullscreen,
                WindowedWidth = WindowedWidth,
                WindowedHeight = WindowedHeight,
                TargetDisplayIndex = TargetDisplayIndex,
                WindowPositionX = WindowPositionX,
                WindowPositionY = WindowPositionY,
                Aspect = Aspect
            };
        }
    }
}
