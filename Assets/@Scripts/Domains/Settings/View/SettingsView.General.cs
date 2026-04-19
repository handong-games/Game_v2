using Game.Core.Managers.Locale;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView
    {
        private DropdownField _languageField;
        
        private void OnBindGeneral()
        {
            _languageField = Bind<DropdownField, string>("language-field", OnLanguageChanged);
            _languageField.choices = LocaleManager.Instance.GetLocaleLabels();
            _languageField.SetValueWithoutNotify(LocaleManager.Instance.CurrentLocale.LocaleName);
        }

        private void OnUnbindGeneral()
        {
            Unbind<DropdownField, string>(_languageField, OnLanguageChanged);
        }

        private void OnLanguageChanged(ChangeEvent<string> evt)
        {
            LocaleManager.Instance.SetLanguage(_languageField.index);
        }
    }
}
