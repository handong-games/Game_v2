namespace Game.Core.Managers.Save
{
    public sealed class ProgressService : SaveService<ProgressState, ProgressSave>
    {
        public ProgressService() : base("progress.json")
        {
        }

        protected override ProgressState FromSave(ProgressSave save)
        {
            return ProgressState.FromSave(save);
        }
    }
}
