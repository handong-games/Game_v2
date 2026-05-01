using System;
using System.Collections.Generic;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public sealed class ProgressSave : SaveData
    {
        public const int CurrentVersion = 1;

        public List<string> CharacterUnlockIds = new()
        {
            "Warrior"
        };

        public ProgressSave() : base(CurrentVersion)
        {
        }
    }
}
