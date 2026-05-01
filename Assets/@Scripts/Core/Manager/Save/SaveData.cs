using System;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public abstract class SaveData
    {
        public int version;

        protected SaveData(int version)
        {
            this.version = version;
        }
    }
}
