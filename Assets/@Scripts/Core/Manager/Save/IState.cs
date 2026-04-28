namespace Game.Core.Managers.Save
{
    public interface IState<TSave> where TSave : ISave
    {
        TSave ToSave();
    }
}
