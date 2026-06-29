using Domains.Adventure;
using Domains.Card;
using Domains.Intent.Execution;
using Domains.Intent.Flow;
using Domains.Intent.Presentation;
using Domains.Intent.Runtime;
using Domains.View.Widgets;
using Game.Core.Navigation;
using Game.Core.Ports;
using Game.Scenes.Adventure.Events;
using Game.Scenes.Adventure.Events.Flow;
using Game.Scenes.Adventure.Events.Widgets;
using VContainer;
using VContainer.Unity;

namespace Game.Scenes.Adventure
{
    // Role:
    // Builds the VContainer scope for one AdventureScene instance.
    // It consumes preloaded scene data and owns its Addressables handles until the scene scope ends.
    public sealed class AdventureSceneScope : LifetimeScope
    {
        protected override LifetimeScope FindParent()
        {
            return global::GameBootstrap.RootLifetimeScope;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            AdventureSceneLoader loader =
                global::GameBootstrap.RootLifetimeScope.Container.Resolve<AdventureSceneLoader>();

            AdventureScenePayload payload = loader.Consume();
            AdventureSceneInitialData initialData = payload.InitialData;
            AdventureChoiceCardUIModels choiceCardUIModels = payload.ChoiceCardUIModels;
            AdventureCardWidgetTemplates cardWidgetTemplates = payload.CardWidgetTemplates;
            AdventureScreenWidgetTemplates screenWidgetTemplates = payload.ScreenWidgetTemplates;
            AdventureSceneAssetHandles assetHandles = payload.AssetHandles;

            builder.RegisterInstance(initialData);
            builder.RegisterInstance(initialData.Character);
            builder.RegisterInstance(initialData.Adventure);
            builder.RegisterInstance(choiceCardUIModels);
            builder.RegisterInstance(cardWidgetTemplates);
            builder.RegisterInstance(screenWidgetTemplates);
            builder.RegisterInstance(assetHandles);
            builder.RegisterDisposeCallback(_ => assetHandles.Dispose());

            builder.Register<AdventureRunState>(Lifetime.Scoped);
            builder.Register<CardRegistry>(Lifetime.Scoped);
            builder.Register<CardFactory>(Lifetime.Scoped);
            builder.Register<CardBoardState>(Lifetime.Scoped);
            builder.Register<AdventureProgress>(Lifetime.Scoped);
            builder.Register<AdventureRegionData>(Lifetime.Scoped);
            builder.Register<AdventureInputState>(Lifetime.Scoped);
            builder.Register<AdventureCards>(Lifetime.Scoped);
            builder.Register<AdventureBoard>(Lifetime.Scoped);
            builder.Register<AdventurePlayer>(Lifetime.Scoped);
            builder.Register<AdventureStageRuntime>(Lifetime.Scoped);
            builder.Register<AdventureEncounterSequenceRuntime>(Lifetime.Scoped);
            builder.Register<AdventureOfferFactory>(Lifetime.Scoped);
            builder.Register<AdventureCombatRuntime>(Lifetime.Scoped);
            builder.Register<AdventureCardAvatarRegistry>(Lifetime.Scoped);
            builder.Register<IntentRuntime>(Lifetime.Scoped);
            builder.Register<ActionExecutionBindingStore>(Lifetime.Scoped);
            builder.Register<IntentActionResolver>(Lifetime.Scoped);
            builder.Register<IntentDisplayBuilder>(Lifetime.Scoped);
            builder.Register<IntentPrepareFlow>(Lifetime.Scoped);
            builder.Register<IntentRefreshFlow>(Lifetime.Scoped);
            builder.Register<IntentConsumeFlow>(Lifetime.Scoped);
            builder.Register<IntentExecutionSetupFlow>(Lifetime.Scoped);
            builder.Register<IntentPresenter>(Lifetime.Scoped);
            builder.Register<AdventureStartFlow>(Lifetime.Scoped);
            builder.Register<AdventureStageFlow>(Lifetime.Scoped);
            builder.Register<AdventureChoiceFlow>(Lifetime.Scoped);
            builder.Register<AdventureStageAdvanceFlow>(Lifetime.Scoped);
            builder.Register<AdventureStageContinuationFlow>(Lifetime.Scoped);
            builder.Register<AdventureEncounterStartFlow>(Lifetime.Scoped);
            builder.Register<AdventureEncounterFlow>(Lifetime.Scoped);
            builder.Register<AdventureCombatEncounterFlow>(Lifetime.Scoped);
            builder.Register<AdventureEventFlow>(Lifetime.Scoped);
            builder.Register<AdventureShopFlow>(Lifetime.Scoped);
            builder.Register<AdventureTurnFlow>(Lifetime.Scoped);
            builder.Register<AdventureCoinFlow>(Lifetime.Scoped);
            builder.Register<AdventureSkillFlow>(Lifetime.Scoped);
            builder.Register<AdventureEnemyActionFlow>(Lifetime.Scoped);
            builder.Register<AdventureEnemyDeathFlow>(Lifetime.Scoped);
            builder.Register<AdventureCombatResultFlow>(Lifetime.Scoped);
            builder.Register<AdventureCardAvatarBindingFlow>(Lifetime.Scoped);
            builder.Register<AdventurePresenter>(Lifetime.Scoped);
            builder.Register<AdventureScreenWidgets>(Lifetime.Scoped);
            builder.Register<AdventureScreenWidgetBinder>(Lifetime.Scoped);
            builder.Register<AdventureBoardWidgets>(Lifetime.Scoped);
            builder.Register<AdventureBoardCardRegistry>(Lifetime.Scoped);
            builder.Register<AdventureCardDealAnimator>(Lifetime.Scoped);
            builder.Register<AdventureBoardLayout>(Lifetime.Scoped);
            builder.Register<AdventureCardWidgetFactory>(Lifetime.Scoped);
            builder.Register<AdventureBoardUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureResourceStatusUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureIntroUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureCoinUIFlow>(Lifetime.Scoped);
            builder.Register<AdventurePlayerTurnUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureEnemyTurnUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureIntentUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureCombatResultUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureRewardUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureChoiceRefreshUIFlow>(Lifetime.Scoped);
            builder.Register<DamageNumberManager>(Lifetime.Scoped);
            builder.Register<DamageNumberUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureScreenUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureSkillUIFlow>(Lifetime.Scoped);
            builder.Register<AdventureGameplayCueAvatarBinder>(Lifetime.Scoped);
            builder.Register<AdventureCardWidgetEventBinder>(Lifetime.Scoped);

            builder.Register<AdventureBoardEvents>(Lifetime.Scoped);
            builder.Register<AdventureScreenEvents>(Lifetime.Scoped);
            builder.Register<AdventureCombatEvents>(Lifetime.Scoped);
            builder.Register<AdventureGameEvents>(Lifetime.Scoped);
            builder.Register<AdventureTurnWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventurePouchWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventureCardWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventureSkillSlotWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventureWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventureSceneLocalization>(Lifetime.Scoped);
            builder.Register<ISceneViewNavigator, SceneViewNavigator>(Lifetime.Scoped);
            builder.Register<AdventureSceneNavigator>(Lifetime.Scoped);
            builder.Register<AdventureScreenController>(Lifetime.Scoped);
            builder.Register<AdventureView>(Lifetime.Scoped);
            builder.Register<AdventureGameToScreenEventBinder>(Lifetime.Scoped);
            builder.Register<AdventureWidgetToScreenEventBinder>(Lifetime.Scoped);
            builder.RegisterEntryPoint<AdventureCombatDeathObserver>();
            builder.RegisterEntryPoint<AdventureSceneEntryPoint>();
        }
    }
}
