using Domains.Adventure;
using Domains.Scene.Adventure;
using Game.Core.Adapters;
using Game.Core.Managers.Audio;
using Game.Core.Managers.DB;
using Game.Core.Managers.Garphic;
using Game.Core.Managers.Locale;
using Game.Core.Managers.Save;
using Game.Core.Managers.View;
using Game.Core.Ports;
using Game.Core.SceneLoading;
using Game.Messages;
using VContainer;
using VContainer.Unity;

namespace Game.Core.Composition
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<DBManager>(Lifetime.Singleton);
            builder.Register<SaveManager>(Lifetime.Singleton);
            builder.Register<GraphicManager>(Lifetime.Singleton);
            builder.Register<AudioManager>(Lifetime.Singleton);
            builder.Register<LocaleManager>(Lifetime.Singleton);
            builder.Register<GameplayMessageManager>(Lifetime.Singleton);
            builder.Register<ViewTransitionManager>(Lifetime.Singleton);
            builder.Register<ViewManager>(Lifetime.Singleton)
                .AsSelf()
                .As<IViewHost>();
            builder.Register<AdventureStartState>(Lifetime.Singleton);
            builder.Register<AdventureSceneLoader>(Lifetime.Singleton);
            builder.Register(
                resolver => new ScenePreloadService(new IScenePreloader[]
                {
                    resolver.Resolve<AdventureSceneLoader>(),
                }),
                Lifetime.Singleton);
            builder.Register<SceneManagerEx>(Lifetime.Singleton);
            builder.Register<ISceneTransitionPlayer, ViewOverlaySceneTransitionPlayer>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<GameRootEntryPoint>(Lifetime.Singleton);
        }
    }
}
