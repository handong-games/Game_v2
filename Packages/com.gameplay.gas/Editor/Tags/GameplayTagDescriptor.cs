namespace Gameplay.GAS.Editor
{
    public readonly struct GameplayTagDescriptor
    {
        public GameplayTagDescriptor(
            string path,
            string providerDisplayName,
            string fieldDisplayName,
            GameplayTag tag)
        {
            Path = path;
            ProviderDisplayName = providerDisplayName;
            FieldDisplayName = fieldDisplayName;
            Tag = tag;
        }

        public string Path { get; }
        public string ProviderDisplayName { get; }
        public string FieldDisplayName { get; }
        public GameplayTag Tag { get; }
    }
}
