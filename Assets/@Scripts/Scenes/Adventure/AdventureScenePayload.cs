namespace Game.Scenes.Adventure
{
    // Role:
    // Transfers preloaded Adventure startup data and its Addressables handles into AdventureSceneScope.
    // After Consume, AdventureSceneScope owns the handles and must release them on scope disposal.
    public sealed class AdventureScenePayload
    {
        public AdventureScenePayload(
            AdventureSceneInitialData initialData,
            AdventureSceneAssetHandles assetHandles)
        {
            InitialData = initialData;
            AssetHandles = assetHandles;
        }

        public AdventureSceneInitialData InitialData { get; }
        public AdventureSceneAssetHandles AssetHandles { get; }
    }
}
