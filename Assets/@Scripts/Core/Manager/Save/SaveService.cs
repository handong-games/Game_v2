namespace Game.Core.Managers.Save
{
    public abstract class SaveService<TState, TSave>
        where TState : IState<TSave>
        where TSave : ISave, new()
    {
        private readonly JsonFileStore<TSave> _store;

        public TState State { get; protected set; }

        protected SaveService(string fileName)
        {
            _store = new JsonFileStore<TSave>(fileName);
        }

        public void Load()
        {
            TSave save = _store.Load();
            State = FromSave(save);
        }

        public void Save()
        {
            _store.Save(State.ToSave());
        }

        protected abstract TState FromSave(TSave save);
    }
}
