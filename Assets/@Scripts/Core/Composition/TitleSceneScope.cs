using Domains.Character;
using Domains.CharacterSelect;
using Domains.Settings;
using Domains.Scene.Title;
using Domains.Settings.View;
using Game.Core.Navigation;
using Game.Core.Ports;
using Views.TitleView;
using VContainer;
using VContainer.Unity;

namespace Game.Core.Composition
{
    public sealed class TitleSceneScope : LifetimeScope
    {
        protected override LifetimeScope FindParent()
        {
            return global::GameBootstrap.RootLifetimeScope;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<TitleSceneBgmOwner>(Lifetime.Scoped);
            builder.Register<TitleSceneLocalizationOwner>(Lifetime.Scoped);
            builder.Register<TitleSceneCardFaceTemplateLoader>(Lifetime.Scoped);
            builder.Register<TitleSceneSkillSlotTemplateLoader>(Lifetime.Scoped);
            builder.Register<ISceneViewNavigator, SceneViewNavigator>(Lifetime.Scoped);
            builder.Register<TitleSceneNavigator>(Lifetime.Scoped);
            builder.RegisterEntryPoint<TitleSceneEntryPoint>();

            // TitleScene views are scene-owned and must flow through
            // TitleSceneNavigator -> ISceneViewNavigator -> IViewHost.
            // Do not use the legacy ViewManager Push/Pop stack in TitleScene.
            builder.Register<TitleView>(Lifetime.Scoped);
            builder.Register<SettingsView>(Lifetime.Scoped);
            builder.Register<CharacterSelectView>(Lifetime.Scoped);

            builder.Register<CharacterService>(Lifetime.Scoped);
            builder.Register<TitleViewController>(Lifetime.Scoped);
            builder.Register<CharacterSelectController>(Lifetime.Scoped);
            builder.Register<ICharacterUnlockGateway, LegacyCharacterUnlockGateway>(Lifetime.Scoped);
            builder.Register<ISettingsGateway, LegacySettingsGateway>(Lifetime.Scoped);
            builder.Register<SettingsViewController>(Lifetime.Scoped);
        }
    }
}
