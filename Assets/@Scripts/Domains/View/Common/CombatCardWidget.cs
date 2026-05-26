using Domains.Adventure;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class CombatCardWidget : VisualElement
    {
        private const string Address = "CombatCardWidget";
        private const string WidgetName = "combat-card";
        private const string CardWidgetName = "combat-card-widget";
        private const string HealthWidgetName = "combat-card-health-widget";

        private static VisualTreeAsset _template;

        private CardWidget _cardWidget;
        private HealthWidget _healthWidget;

        public CombatCardWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _cardWidget = this.Q<CardWidget>(CardWidgetName);
            _healthWidget = this.Q<HealthWidget>(HealthWidgetName);
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Unbind();
        }

        public static CombatCardWidget Create()
        {
            TemplateContainer container = LoadTemplate().CloneTree();
            CombatCardWidget widget = container.Q<CombatCardWidget>(WidgetName);
            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(CardViewModel card, AbilitySystemComponent abilitySystem)
        {
            Unbind();
            _cardWidget.Bind(card);
            _healthWidget.Bind(abilitySystem);
        }

        public async Awaitable ShowHealthAsync()
        {
            if (_healthWidget == null)
                return;

            await _healthWidget.Show();
        }

        public void Unbind()
        {
            _cardWidget?.Unbind();
            _healthWidget?.Unbind();
        }

        private static VisualTreeAsset LoadTemplate()
        {
            if (_template != null)
                return _template;

            _template = Addressables.LoadAssetAsync<VisualTreeAsset>(Address).WaitForCompletion();
            return _template;
        }
    }
}
