using Domains.Adventure;
using Game.Core.Managers.View;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class AdventurePlayerCardWidget :
        VisualElement,
        IAdventureDamageCueReceiver,
        IAdventureHealthCardWidget
    {
        private const string WidgetName = "adventure-player-card";
        private const string CardWidgetName = "adventure-player-card-widget";
        private const string HealthWidgetName = "adventure-player-card-health-widget";

        private CardWidget _cardWidget;
        private HealthWidget _healthWidget;

        public AdventurePlayerCardWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static AdventurePlayerCardWidget Create(VisualTreeAsset template)
        {
            if (template == null)
                throw new System.ArgumentNullException(nameof(template));

            TemplateContainer container = template.Instantiate();
            AdventurePlayerCardWidget widget = container.Q<AdventurePlayerCardWidget>(WidgetName);
            if (widget == null)
                throw new System.InvalidOperationException($"Required element missing: {WidgetName}");

            widget.RemoveFromHierarchy();
            widget.InitializeReferences();
            return widget;
        }

        public VisualElement InteractionTarget
        {
            get
            {
                EnsureReferences();
                return _cardWidget;
            }
        }

        public void Bind(
            CardViewModel card,
            AbilitySystemComponent abilitySystem,
            AdventureCardWidgetTemplates templates)
        {
            if (templates == null)
                throw new System.ArgumentNullException(nameof(templates));

            EnsureReferences();
            if (abilitySystem == null)
                throw new System.ArgumentNullException(nameof(abilitySystem));

            Unbind();
            _cardWidget.Bind(card, templates.FaceTemplates);
            _healthWidget.Bind(abilitySystem);
        }

        public async Awaitable ShowHealthAsync(ViewTransitionManager transitionManager)
        {
            EnsureReferences();
            await _healthWidget.Show(transitionManager);
        }

        public void SetHealth(int currentHealth, int maxHealth)
        {
            EnsureReferences();
            _healthWidget.SetMaxHealth(maxHealth);
            _healthWidget.SetHealth(currentHealth);
        }

        public void Unbind()
        {
            _cardWidget?.Unbind();
            _healthWidget?.Unbind();
        }

        public async void HandleDamageCue(DamageCueData data)
        {
            try
            {
                EnsureReferences();
                if (data == null)
                    return;

                await _healthWidget.PlayDamageCue(data.Amount);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            EnsureReferences();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Unbind();
        }

        private void InitializeReferences()
        {
            _healthWidget ??= this.Q<HealthWidget>(HealthWidgetName);
            _cardWidget ??= this.Q<CardWidget>(CardWidgetName);
        }

        private void EnsureReferences()
        {
            InitializeReferences();

            if (_cardWidget == null)
                throw new System.InvalidOperationException($"Required element missing: {CardWidgetName}");

            if (_healthWidget == null)
                throw new System.InvalidOperationException($"Required element missing: {HealthWidgetName}");
        }
    }
}
