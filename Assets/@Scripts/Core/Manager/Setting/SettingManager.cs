using System;
using System.IO;
using Domains.Settings;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    public sealed class SettingManager : BaseManager<SettingManager>
    {
        private const string SettingsFileName = "settings.json";

        private string _settingsPath;
        private SettingsSaveData _saveData;

        public SettingsSaveData SaveData => _saveData;

        protected override void OnInit()
        {
            Application.quitting += Save;

            _settingsPath = Path.Combine(Application.persistentDataPath, SettingsFileName);
            if (!File.Exists(_settingsPath))
            {
                CreateDefaultSaveData();
                return;
            }

            try
            {
                string json = File.ReadAllText(_settingsPath);
                SettingsSaveData loaded = JsonUtility.FromJson<SettingsSaveData>(json) ?? new SettingsSaveData();
                loaded.Normalize();
                _saveData = loaded;
            }
            catch
            {
                CreateDefaultSaveData();
            }
        }

        protected override void OnDispose()
        {
            Application.quitting -= Save;
            _saveData = null;
            _settingsPath = null;
        }

        public void Save()
        {
            _saveData.Normalize();
            string json = JsonUtility.ToJson(_saveData, true);
            string tempPath = _settingsPath + ".tmp";

            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(tempPath, json);

            if (File.Exists(_settingsPath))
                File.Delete(_settingsPath);

            File.Move(tempPath, _settingsPath);
        }

        private void CreateDefaultSaveData()
        {
            SettingsSaveData newSaveData = new SettingsSaveData();
            newSaveData.Normalize();
            _saveData = newSaveData;
            Save();
        }
    }
}
