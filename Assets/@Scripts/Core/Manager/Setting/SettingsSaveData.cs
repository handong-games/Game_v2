using System;
using Game.Core.Managers.Save;

namespace Domains.Settings
{
    [Serializable]
    public sealed partial class SettingsSaveData
    {
        public const int CurrentVersion = 1;
        
        public int SaveVersion = 1;

        public void Normalize()
        {
            SaveVersion = CurrentVersion;
            
            NormalizeAudio();
            NormalizeDisplay();
            NormalizeGeneral();
        }
    }
}
