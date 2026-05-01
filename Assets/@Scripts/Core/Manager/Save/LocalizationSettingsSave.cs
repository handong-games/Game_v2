using System;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public sealed class LocalizationSettingsSave : SaveData
    {
        public const int CurrentVersion = 1;

        public string LanguageCode = "en-US";

        public LocalizationSettingsSave() : base(CurrentVersion)
        {
        }
    }
}
