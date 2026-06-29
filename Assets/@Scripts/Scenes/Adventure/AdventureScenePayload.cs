namespace Game.Scenes.Adventure
{
    using Domains.View.Widgets;
    using System;

    // Role:
    // Transfers preloaded Adventure startup data and its Addressables handles into AdventureSceneScope.
    // After Consume, AdventureSceneScope owns the handles and must release them on scope disposal.
    public sealed class AdventureScenePayload
    {
        public AdventureScenePayload(
            AdventureSceneInitialData initialData,
            AdventureChoiceCardUIModels choiceCardUIModels,
            AdventureCardWidgetTemplates cardWidgetTemplates,
            AdventureScreenWidgetTemplates screenWidgetTemplates,
            AdventureSceneAssetHandles assetHandles)
        {
            InitialData = initialData ?? throw new ArgumentNullException(nameof(initialData));
            ChoiceCardUIModels = choiceCardUIModels ?? throw new ArgumentNullException(nameof(choiceCardUIModels));
            CardWidgetTemplates = cardWidgetTemplates ?? throw new ArgumentNullException(nameof(cardWidgetTemplates));
            ScreenWidgetTemplates = screenWidgetTemplates ?? throw new ArgumentNullException(nameof(screenWidgetTemplates));
            AssetHandles = assetHandles ?? throw new ArgumentNullException(nameof(assetHandles));
        }

        public AdventureSceneInitialData InitialData { get; }
        public AdventureChoiceCardUIModels ChoiceCardUIModels { get; }
        public AdventureCardWidgetTemplates CardWidgetTemplates { get; }
        public AdventureScreenWidgetTemplates ScreenWidgetTemplates { get; }
        public AdventureSceneAssetHandles AssetHandles { get; }
    }
}
