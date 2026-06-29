using System.Collections.Generic;
using Domains.Adventure;
using Domains.Intent.Presentation;
using Game.Core.Managers.View;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class AdventureMonsterCardWidget :
        VisualElement,
        IAdventureDamageCueReceiver,
        IAdventureHealthCardWidget,
        IAdventureIntentCardWidget
    {
        private const string WidgetName = "adventure-monster-card";
        private const string CardWidgetName = "adventure-monster-card-widget";
        private const string HealthWidgetName = "adventure-monster-card-health-widget";
        private const string IntentBadgeName = "adventure-monster-card-intent-badge";

        private CardWidget _cardWidget;
        private HealthWidget _healthWidget;
        private IntentBadgeWidget _intentBadgeWidget;

        public AdventureMonsterCardWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static AdventureMonsterCardWidget Create(VisualTreeAsset template)
        {
            if (template == null)
                throw new System.ArgumentNullException(nameof(template));

            TemplateContainer container = template.Instantiate();
            AdventureMonsterCardWidget widget = container.Q<AdventureMonsterCardWidget>(WidgetName);
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

        public async Awaitable ShowIntentAsync(IReadOnlyList<IntentItemViewModel> items)
        {
            EnsureReferences();
            if (items == null || items.Count == 0)
                throw new System.InvalidOperationException("Monster intent items are missing.");

            await _intentBadgeWidget.Show(items[0]);
        }

        public async Awaitable RefreshIntentAsync(IReadOnlyList<IntentItemViewModel> items)
        {
            EnsureReferences();
            if (items == null || items.Count == 0)
                throw new System.InvalidOperationException("Monster intent items are missing.");

            await _intentBadgeWidget.Refresh(items[0]);
        }

        public Awaitable TriggerIntentAsync()
        {
            EnsureReferences();
            return _intentBadgeWidget.Trigger();
        }

        public void Unbind()
        {
            _cardWidget?.Unbind();
            _healthWidget?.Unbind();
            _intentBadgeWidget?.Hide();
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
            _intentBadgeWidget ??= this.Q<IntentBadgeWidget>(IntentBadgeName);
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

            if (_intentBadgeWidget == null)
                throw new System.InvalidOperationException($"Required element missing: {IntentBadgeName}");
        }
    }
}
