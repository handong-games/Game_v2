using System.Collections.Generic;
using Domains.Adventure;
using Domains.Intent.Presentation;
using Game.Scenes.Adventure.Events.Widgets;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class AdventureMonsterCardWidget : VisualElement, IAdventureGameplayCueReceiver
    {
        private const string Address = "AdventureMonsterCardWidget";
        private const string WidgetName = "adventure-monster-card";
        private const string CardWidgetName = "adventure-monster-card-widget";
        private const string HealthWidgetName = "adventure-monster-card-health-widget";
        private const string IntentBadgeName = "adventure-monster-card-intent-badge";

        private static VisualTreeAsset _template;

        private CardWidget _cardWidget;
        private HealthWidget _healthWidget;
        private IntentBadgeWidget _intentBadgeWidget;

        public AdventureMonsterCardWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static AdventureMonsterCardWidget Create()
        {
            TemplateContainer container = LoadTemplate().Instantiate();
            AdventureMonsterCardWidget widget = container.Q<AdventureMonsterCardWidget>(WidgetName);
            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(
            CardViewModel card,
            AbilitySystemComponent abilitySystem,
            IntentBadgeWidgetEvents intentBadgeEvents)
        {
            Unbind();
            _intentBadgeWidget?.Bind(intentBadgeEvents);
            _cardWidget?.Bind(card);
            _healthWidget?.Bind(abilitySystem);
        }

        public async Awaitable ShowHealthAsync()
        {
            if (_healthWidget == null)
                return;

            await _healthWidget.Show();
        }

        public async Awaitable ShowIntentAsync(IReadOnlyList<IntentItemViewModel> items)
        {
            if (_intentBadgeWidget == null || items == null || items.Count == 0)
                return;

            await _intentBadgeWidget.Show(items[0]);
        }

        public async Awaitable RefreshIntentAsync(IReadOnlyList<IntentItemViewModel> items)
        {
            if (_intentBadgeWidget == null || items == null || items.Count == 0)
                return;

            await _intentBadgeWidget.Refresh(items[0]);
        }

        public Awaitable TriggerIntentAsync()
        {
            return _intentBadgeWidget != null
                ? _intentBadgeWidget.Trigger()
                : Awaitable.NextFrameAsync();
        }

        public void Unbind()
        {
            _cardWidget?.Unbind();
            _healthWidget?.Unbind();
            _intentBadgeWidget?.Hide();
        }

        public void HandleCoinFlipCue(CoinFlipCueData data)
        {
        }

        public void HandleCoinChangeCue(CoinChangeCueData data)
        {
        }

        public void HandleDamageCue(DamageCueData data)
        {
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _intentBadgeWidget = this.Q<IntentBadgeWidget>(IntentBadgeName);
            _healthWidget = this.Q<HealthWidget>(HealthWidgetName);
            _cardWidget = this.Q<CardWidget>(CardWidgetName);
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
