using Domains.Adventure;
using Domains.Combat;
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
        private CardHealthBinding _healthBinding;

        public CombatCardWidget()
        {
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static CombatCardWidget Create()
        {
            TemplateContainer container = LoadTemplate().CloneTree();
            CombatCardWidget widget = container.Q<CombatCardWidget>(WidgetName);
            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(CardViewModel card, CardHealthBinding health)
        {
            ResolveElements();
            Unbind();

            _cardWidget.Bind(card);
            BindHealth(health);
        }

        public async Awaitable ShowHealthAsync()
        {
            ResolveElements();

            if (_healthBinding == null || !_healthBinding.IsValid)
                return;

            await _healthWidget.Show();
        }

        public void Unbind()
        {
            _cardWidget?.Unbind();
            UnbindHealth();
        }

        private void BindHealth(CardHealthBinding health)
        {
            if (health == null || !health.IsValid)
                return;

            _healthBinding = health;
            _healthWidget.SetHealth(_healthBinding.CurrentHealth, _healthBinding.MaxHealth);
            _healthBinding.HealthChanged += OnHealthChanged;
            _healthBinding.HealthPreviewChanged += OnHealthPreviewChanged;
        }

        private void UnbindHealth()
        {
            if (_healthBinding == null)
                return;

            _healthBinding.HealthChanged -= OnHealthChanged;
            _healthBinding.HealthPreviewChanged -= OnHealthPreviewChanged;
            _healthBinding = null;
            _healthWidget.Hide();
            _healthWidget.HidePreview();
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (panel == null)
                return;

            _healthWidget.SetHealth(currentHealth, maxHealth);
        }

        private void OnHealthPreviewChanged(HealthPreviewDto preview)
        {
            if (panel == null)
                return;

            if (preview.IsEnabled)
            {
                _healthWidget.ShowPreview(preview);
                return;
            }

            _healthWidget.HidePreview();
        }

        private void ResolveElements()
        {
            _cardWidget ??= this.Q<CardWidget>(CardWidgetName);
            _healthWidget ??= this.Q<HealthWidget>(HealthWidgetName);
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Unbind();
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
