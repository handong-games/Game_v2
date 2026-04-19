using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine;

namespace Domains.Settings
{
    [Serializable]
    public class LocaleSaveData
    {
        public string languageCode;
    }
    
    public sealed partial class SettingsSaveData
    {
        [SerializeField]
        private LocaleSaveData  _localeSaveData = new LocaleSaveData();
        
        public LocaleSaveData LocaleSaveData => _localeSaveData;
        
        private void NormalizeGeneral()
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(_localeSaveData.languageCode);
            if (locale == null)
            {
                locale = new SystemLocaleSelector().GetStartupLocale(LocalizationSettings.AvailableLocales);
                if (locale == null)
                {
                    locale = LocalizationSettings.AvailableLocales.GetLocale("en-US");
                }

                _localeSaveData.languageCode = locale.Identifier.Code;
            }
        }
    }
}
