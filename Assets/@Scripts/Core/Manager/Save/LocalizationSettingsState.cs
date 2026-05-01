using Game.Core.Managers.Dependency;
using UnityEngine.Localization.Settings;

namespace Game.Core.Managers.Save
{
    [Dependency]
    public sealed class LocalizationSettingsState : ISave<LocalizationSettingsSave>
    {
        public string LanguageCode { get; set; } = "en-US";

        public void LoadFrom(LocalizationSettingsSave save)
        {
            LanguageCode = save.LanguageCode;

            UnityEngine.Localization.Locale locale = LocalizationSettings.AvailableLocales.GetLocale(LanguageCode);
            if (locale != null)
                return;

            locale = new SystemLocaleSelector().GetStartupLocale(LocalizationSettings.AvailableLocales)
                ?? LocalizationSettings.AvailableLocales.GetLocale("en-US");

            if (locale != null)
            {
                LanguageCode = locale.Identifier.Code;
            }
        }

        public LocalizationSettingsSave ToSave()
        {
            return new LocalizationSettingsSave
            {
                LanguageCode = LanguageCode
            };
        }
    }
}
