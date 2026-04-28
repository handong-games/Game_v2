using UnityEngine;

namespace Game.Core.Managers.Save
{
    public sealed class SaveManager : BaseManager<SaveManager>
    {
        private SettingsService _settingsService;
        private ProgressService _progressService;

        public SettingsState Settings => _settingsService.State;
        public ProgressState Progress => _progressService.State;

        protected override void OnInit()
        {
            Application.quitting += SaveAll;

            _settingsService = new SettingsService();
            _progressService = new ProgressService();

            _settingsService.Load();
            _progressService.Load();
        }

        protected override void OnDispose()
        {
            Application.quitting -= SaveAll;

            _settingsService = null;
            _progressService = null;
        }

        public void SaveSettings()
        {
            _settingsService.Save();
        }

        public void SaveProgress()
        {
            _progressService.Save();
        }

        public void SaveAll()
        {
            SaveSettings();
            SaveProgress();
        }
    }
}
