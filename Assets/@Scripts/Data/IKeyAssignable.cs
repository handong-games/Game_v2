namespace Game.Data
{
    public interface IKeyAssignable<TKey> where TKey : global::System.Enum
    {
        TKey Key { get; }
        void SetKey(TKey key);
    }
}
