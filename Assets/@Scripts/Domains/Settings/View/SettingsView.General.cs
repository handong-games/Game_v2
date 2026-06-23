using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView
    {
        private DropdownField _languageField;
        
        private void OnBindGeneral()
        {
            _languageField = Bind<DropdownField, string>("language-field", OnLanguageChanged);
        }

        private void RefreshGeneral()
        {
            _languageField.choices = new List<string>(_controller.GetLocaleLabels());
            _languageField.SetValueWithoutNotify(_controller.GetCurrentLocaleName());
        }

        private void OnUnbindGeneral()
        {
            Unbind<DropdownField, string>(_languageField, OnLanguageChanged);
        }

        private void OnLanguageChanged(ChangeEvent<string> evt)
        {
            _controller.SetLanguage(_languageField.index);
        }
    }
}
