using System.Collections.Generic;

namespace Domains.Settings
{
    public interface ISettingsGateway
    {
        void SaveAll();

        IReadOnlyList<string> GetLocaleLabels();
        string GetCurrentLocaleName();
        void SetLanguage(int localeIndex);

        bool GetMuteInBackground();
        void SetMuteInBackground(bool enabled);
        float GetVolume(EAudioVolume volume);
        void SetVolume(EAudioVolume volume, float value);

        bool IsFullscreen();
        void SetFullscreen(bool isFullscreen);
        string GetCurrentResolutionText();
        IReadOnlyList<string> GetResolutionLabels();
        bool SetResolutionAtIndex(int index);
        string GetAspectPresetText();
        IReadOnlyList<string> GetAspectPresetLabels();
        bool SetAspectPresetAtIndex(int index);
    }
}
