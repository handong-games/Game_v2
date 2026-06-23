using System.Collections.Generic;
using Game.Core.Define;
using Game.Core.Managers.Audio;
using Game.Core.Managers.Garphic;
using Game.Core.Managers.Locale;
using Game.Core.Managers.Save;
using UnityEngine;

namespace Domains.Settings
{
    public sealed class LegacySettingsGateway : ISettingsGateway
    {
        private readonly SaveManager _saveManager;
        private readonly LocaleManager _localeManager;
        private readonly AudioManager _audioManager;
        private readonly GraphicManager _graphicManager;

        public LegacySettingsGateway(
            SaveManager saveManager,
            LocaleManager localeManager,
            AudioManager audioManager,
            GraphicManager graphicManager)
        {
            _saveManager = saveManager;
            _localeManager = localeManager;
            _audioManager = audioManager;
            _graphicManager = graphicManager;
        }

        public void SaveAll()
        {
            _saveManager.SaveAll();
        }

        public IReadOnlyList<string> GetLocaleLabels()
        {
            return _localeManager.GetLocaleLabels();
        }

        public string GetCurrentLocaleName()
        {
            return _localeManager.CurrentLocale.LocaleName;
        }

        public void SetLanguage(int localeIndex)
        {
            if (localeIndex < 0)
                return;

            _localeManager.SetLanguage(localeIndex);
        }

        public bool GetMuteInBackground()
        {
            return _audioManager.GetMuteInBackground();
        }

        public void SetMuteInBackground(bool enabled)
        {
            _audioManager.SetMuteInBackground(enabled);
        }

        public float GetVolume(EAudioVolume volume)
        {
            return _audioManager.GetVolume(volume);
        }

        public void SetVolume(EAudioVolume volume, float value)
        {
            _audioManager.SetVolume(volume, value);
        }

        public bool IsFullscreen()
        {
            return _graphicManager.IsFullscreen();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            _graphicManager.SetFullscreen(isFullscreen);
        }

        public string GetCurrentResolutionText()
        {
            return _graphicManager.GetCurrentResolutionText();
        }

        public IReadOnlyList<string> GetResolutionLabels()
        {
            IReadOnlyList<Vector2Int> resolutions = _graphicManager.GetResolutions();
            List<string> labels = new(resolutions.Count);

            for (int i = 0; i < resolutions.Count; i++)
            {
                Vector2Int resolution = resolutions[i];
                labels.Add($"{resolution.x} x {resolution.y}");
            }

            return labels;
        }

        public bool SetResolutionAtIndex(int index)
        {
            IReadOnlyList<Vector2Int> resolutions = _graphicManager.GetResolutions();
            if (index < 0 || index >= resolutions.Count)
                return false;

            Vector2Int resolution = resolutions[index];
            _graphicManager.SetResolution(resolution.x, resolution.y);
            return true;
        }

        public string GetAspectPresetText()
        {
            return _graphicManager.GetAspectPresetText();
        }

        public IReadOnlyList<string> GetAspectPresetLabels()
        {
            return _graphicManager.GetAspectPresetLabels();
        }

        public bool SetAspectPresetAtIndex(int index)
        {
            if (!_graphicManager.TryGetAspectPresetAtIndex(index, out EDisplayAspect preset))
                return false;

            _graphicManager.SetAspectPreset(preset);
            return true;
        }
    }
}
