using System;
using System.Collections.Generic;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public sealed class ProgressSave : ISave
    {
        public int SaveVersion = 1;
        public List<string> CharacterUnlockIds = new()
        {
            "Warrior"
        };
    }
}
