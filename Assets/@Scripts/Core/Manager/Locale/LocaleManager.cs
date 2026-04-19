using System.Collections.Generic;
using Domains.Settings;
using Game.Core.Managers.Save;
using UnityEngine.Localization.Settings;

namespace Game.Core.Managers.Locale
{
    public class LocaleManager : BaseManager<LocaleManager>
    {
        private LocaleSaveData _saveData;
        private UnityEngine.Localization.Locale _currentLocale;
        public UnityEngine.Localization.Locale CurrentLocale => _currentLocale;
        
        protected override void OnInit()
        {
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            
            _saveData = SettingManager.Instance.SaveData.LocaleSaveData;
            _currentLocale = LocalizationSettings.AvailableLocales.GetLocale(_saveData.languageCode);
            LocalizationSettings.SelectedLocale = _currentLocale;
        }

        protected override void OnDispose()
        {
            
        }
        
        public List<string> GetLocaleLabels()
        {
            List<string> localeLabels = new List<string>();
            
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                localeLabels.Add(locale.LocaleName);
            }

            return localeLabels;
        }
        
        public void SetLanguage(int localeIndex)
        {
            if (LocalizationSettings.AvailableLocales.Locales.Count < localeIndex)
                return;

            UnityEngine.Localization.Locale newLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
            SetLocale(newLocale);
        }

        private void SetLocale(UnityEngine.Localization.Locale newLocale)
        {
            _currentLocale = newLocale;
            _saveData.languageCode = newLocale.Identifier.Code;
            LocalizationSettings.SelectedLocale = newLocale;
        }
    }
}