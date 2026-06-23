using System.Collections.Generic;
using System;
using Domains.Settings;
using Game.Core.Managers.Save;
using UnityEngine.Localization.Settings;

namespace Game.Core.Managers.Locale
{
    public class LocaleManager : IDisposable
    {
        private LocalizationSettingsState _settings;
        private readonly SaveManager _saveManager;
        private UnityEngine.Localization.Locale _currentLocale;
        private bool _initialized;

        public UnityEngine.Localization.Locale CurrentLocale => _currentLocale;
        
        public LocaleManager(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            LocalizationSettings.InitializationOperation.WaitForCompletion();
            _settings = _saveManager.GetState<LocalizationSettingsState>();
            _currentLocale = LocalizationSettings.AvailableLocales.GetLocale(_settings.LanguageCode);
            LocalizationSettings.SelectedLocale = _currentLocale;
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _initialized = false;
            _settings = null;
            _currentLocale = null;
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
            _settings.LanguageCode = newLocale.Identifier.Code;
            LocalizationSettings.SelectedLocale = newLocale;
        }
    }
}
