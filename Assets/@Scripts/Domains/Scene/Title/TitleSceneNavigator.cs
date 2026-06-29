using Domains.CharacterSelect;
using Domains.Settings.View;
using Game.Core.Managers.View;
using Game.Core.Ports;
using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using Views.TitleView;

namespace Domains.Scene.Title
{
    public sealed class TitleSceneNavigator
    {
        private readonly ISceneViewNavigator _viewNavigator;
        private readonly IObjectResolver _resolver;
        private readonly TitleSceneCardFaceTemplateLoader _cardFaceTemplateLoader;
        private readonly TitleSceneSkillSlotTemplateLoader _skillSlotTemplateLoader;

        public TitleSceneNavigator(
            ISceneViewNavigator viewNavigator,
            IObjectResolver resolver,
            TitleSceneCardFaceTemplateLoader cardFaceTemplateLoader,
            TitleSceneSkillSlotTemplateLoader skillSlotTemplateLoader)
        {
            _viewNavigator = viewNavigator;
            _resolver = resolver;
            _cardFaceTemplateLoader = cardFaceTemplateLoader;
            _skillSlotTemplateLoader = skillSlotTemplateLoader;
        }
        
        public void ShowTitle()
        {
            TitleView titleView = _resolver.Resolve<TitleView>();
            _viewNavigator.Show(titleView);
        }

        public async Awaitable ShowCharacterSelect()
        {
            try
            {
                CardFaceWidgetTemplates templates = await _cardFaceTemplateLoader.Load();
                VisualTreeAsset skillSlotTemplate = await _skillSlotTemplateLoader.Load();
                CharacterSelectView characterSelectView = _resolver.Resolve<CharacterSelectView>();
                characterSelectView.BindCardFaceTemplates(templates);
                characterSelectView.BindSkillSlotTemplate(skillSlotTemplate);
                _viewNavigator.Show(characterSelectView);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public void ShowSettings()
        {
            SettingsView settingsView = _resolver.Resolve<SettingsView>();
            _viewNavigator.Show(settingsView);
        }

        public void HideCurrent()
        {
            _viewNavigator.HideCurrent();
        }
    }
}
