namespace Domains.Scene.Adventure
{
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
