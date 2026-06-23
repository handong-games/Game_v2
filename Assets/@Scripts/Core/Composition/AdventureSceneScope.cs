using Domains.Adventure;
using Domains.Card;
using Domains.Combat;
using Domains.Scene.Adventure;
using Game.Core.Navigation;
using Game.Core.Ports;
using VContainer;
using VContainer.Unity;

namespace Game.Core.Composition
{
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
            AdventureSceneAssetHandles assetHandles = payload.AssetHandles;

            builder.RegisterInstance(initialData);
            builder.RegisterInstance(initialData.Character);
            builder.RegisterInstance(initialData.Adventure);
            builder.RegisterInstance(initialData.CardDeck);
            builder.RegisterInstance(assetHandles);
            builder.RegisterDisposeCallback(_ => assetHandles.Dispose());

            builder.Register<AdventureRunState>(Lifetime.Scoped);
            builder.Register<AdventureService>(Lifetime.Scoped);
            builder.Register<CardDeckState>(Lifetime.Scoped);
            builder.Register<CardDeckBuilder>(Lifetime.Scoped);
            builder.Register<CardDeckService>(Lifetime.Scoped);
            builder.Register<CardRegistry>(Lifetime.Scoped);
            builder.Register<CardFactory>(Lifetime.Scoped);
            builder.Register<CardService>(Lifetime.Scoped);
            builder.Register<CardBoardState>(Lifetime.Scoped);
            builder.Register<CardBoardService>(Lifetime.Scoped);
            builder.Register<Domains.Player.PlayerRunState>(Lifetime.Scoped);
            builder.Register<Domains.Player.PlayerService>(Lifetime.Scoped);

            builder.Register<AdventureBoardEvents>(Lifetime.Scoped);
            builder.Register<AdventureCombatEvents>(Lifetime.Scoped);
            builder.Register<AdventureEvents>(Lifetime.Scoped);
            builder.Register<AdventureTurnWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventurePouchWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventureSkillSlotWidgetEvents>(Lifetime.Scoped);
            builder.Register<AdventureWidgetEvents>(Lifetime.Scoped);
            builder.Register<CombatService>(Lifetime.Scoped);
            builder.Register<AdventureSceneLocalization>(Lifetime.Scoped);
            builder.Register<AdventureViewEventBinder>(Lifetime.Scoped);
            builder.Register<ISceneViewNavigator, SceneViewNavigator>(Lifetime.Scoped);
            builder.Register<AdventureSceneNavigator>(Lifetime.Scoped);
            builder.Register<AdventureController>(Lifetime.Scoped);
            builder.Register<AdventureView>(Lifetime.Scoped);
            builder.RegisterEntryPoint<AdventureSceneEntryPoint>();
        }
    }
}
