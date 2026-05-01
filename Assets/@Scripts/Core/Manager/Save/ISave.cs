namespace Game.Core.Managers.Save
{
    public interface ISave<TSave>
        where TSave : SaveData, new()
    {
        void LoadFrom(TSave save);
        TSave ToSave();
    }
}
