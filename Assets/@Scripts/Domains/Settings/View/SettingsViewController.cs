using System.Collections.Generic;
using Domains.Scene.Title;
using Domains.Settings;

namespace Domains.Settings.View
{
    public sealed class SettingsViewController
    {
        private readonly ISettingsGateway _settings;
        private readonly TitleSceneNavigator _navigator;

        public SettingsViewController(
            ISettingsGateway settings,
            TitleSceneNavigator navigator)
        {
            _settings = settings;
            _navigator = navigator;
        }

        public void OnClose()
        {
            _settings.SaveAll();
            _navigator.HideCurrent();
        }

        public IReadOnlyList<string> GetLocaleLabels()
        {
            return _settings.GetLocaleLabels();
        }

        public string GetCurrentLocaleName()
        {
            return _settings.GetCurrentLocaleName();
        }

        public void SetLanguage(int localeIndex)
        {
            _settings.SetLanguage(localeIndex);
        }

        public bool GetMuteInBackground()
        {
            return _settings.GetMuteInBackground();
        }

        public void SetMuteInBackground(bool enabled)
        {
            _settings.SetMuteInBackground(enabled);
        }

        public float GetVolume(EAudioVolume volume)
        {
            return _settings.GetVolume(volume);
        }

        public void SetVolume(EAudioVolume volume, float value)
        {
            _settings.SetVolume(volume, value);
        }

        public bool IsFullscreen()
        {
            return _settings.IsFullscreen();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            _settings.SetFullscreen(isFullscreen);
        }

        public string GetCurrentResolutionText()
        {
            return _settings.GetCurrentResolutionText();
        }

        public IReadOnlyList<string> GetResolutionLabels()
        {
            return _settings.GetResolutionLabels();
        }

        public bool SetResolutionAtIndex(int index)
        {
            return _settings.SetResolutionAtIndex(index);
        }

        public string GetAspectPresetText()
        {
            return _settings.GetAspectPresetText();
        }

        public IReadOnlyList<string> GetAspectPresetLabels()
        {
            return _settings.GetAspectPresetLabels();
        }

        public bool SetAspectPresetAtIndex(int index)
        {
            return _settings.SetAspectPresetAtIndex(index);
        }
    }
}
