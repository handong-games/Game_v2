namespace Game.Data
{
    public interface IKeyAssignable<TKey> where TKey : global::System.Enum
    {
        TKey Id { get; }
        void SetId(TKey id);
    }
}
