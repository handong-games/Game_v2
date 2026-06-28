using Domains.Adventure;
using Domains.Card;
using Game.Scenes.Adventure.Events.Widgets;
using Game.Core.Managers.DB;
using Game.Data;
using Gameplay.GAS;
using UnityEngine.UIElements;
using CardActor = Domains.Card.Card;

namespace Domains.View.Widgets
{
    // Role:
    // Creates concrete adventure card widgets. It does not decide which card type is needed.
    public sealed class AdventureCardWidgetFactory
    {
        private readonly DBManager _dbManager;
        private readonly AdventureCards _cards;
        private readonly AdventureCardAvatarRegistry _avatarRegistry;
        private readonly IntentBadgeWidgetEvents _intentBadgeEvents;

        public AdventureCardWidgetFactory(
            DBManager dbManager,
            AdventureCards cards,
            AdventureCardAvatarRegistry avatarRegistry,
            IntentBadgeWidgetEvents intentBadgeEvents)
        {
            _dbManager = dbManager;
            _cards = cards;
            _avatarRegistry = avatarRegistry;
            _intentBadgeEvents = intentBadgeEvents;
        }

        public AdventureBoardCardWidgetBinding Create(
            uint boardCardId,
            AdventureBoardCardViewModel viewModel)
        {
            return viewModel switch
            {
                AdventureChoiceCardViewModel choice => CreateChoiceCard(boardCardId, choice),
                AdventureBoardCardViewModel boardCard => CreateBoardCard(boardCard),
                _ => throw new System.ArgumentOutOfRangeException(nameof(viewModel), viewModel, null),
            };
        }

        public AdventureChoiceCardWidget CreateChoiceCard(
            uint boardCardId,
            AdventureChoiceCardViewModel viewModel,
            AdventureChoiceCardUIModel uiModel)
        {
            AdventureChoiceCardWidget widget = new();
            widget.Bind(boardCardId, viewModel, uiModel);
            return widget;
        }

        private AdventureBoardCardWidgetBinding CreateChoiceCard(
            uint boardCardId,
            AdventureChoiceCardViewModel viewModel)
        {
            if (_dbManager.ChoiceCardUI == null)
            {
                throw new System.InvalidOperationException(
                    "AdventureChoiceCardUITable is not loaded. Create and label the table asset before rendering choice cards.");
            }

            AdventureChoiceCardUIModel uiModel =
                _dbManager.ChoiceCardUI.Get(viewModel.ChoiceType);

            AdventureChoiceCardWidget widget =
                CreateChoiceCard(boardCardId, viewModel, uiModel);

            return new AdventureBoardCardWidgetBinding(
                viewModel,
                widget,
                onPlaced: null,
                dispose: widget.Unbind);
        }

        private AdventureBoardCardWidgetBinding CreateBoardCard(
            AdventureBoardCardViewModel viewModel)
        {
            if (!_cards.TryGet(viewModel.CardId, out CardActor cardActor))
            {
                throw new System.InvalidOperationException(
                    $"Adventure card runtime object is missing: {viewModel.CardId}");
            }

            AdventureMonsterCardWidget widget = AdventureMonsterCardWidget.Create();
            widget.Bind(viewModel.Card, cardActor.AbilitySystem, _intentBadgeEvents);

            AbilitySystemComponent abilitySystem = cardActor.AbilitySystem;
            VisualElement boundTimelineTarget = null;

            abilitySystem?.SetAvatar(widget);
            _avatarRegistry.Register(viewModel.CardId, widget);

            return new AdventureBoardCardWidgetBinding(
                viewModel,
                widget,
                onPlaced: placement =>
                {
                    if (cardActor == null || cardActor.Timeline.IsBound)
                        return;

                    boundTimelineTarget = placement.Anchor;
                    cardActor.Timeline.Bind(boundTimelineTarget);
                },
                dispose: () =>
                {
                    if (cardActor?.Timeline.IsBound == true && boundTimelineTarget != null)
                    {
                        cardActor.Timeline.Release(boundTimelineTarget);
                        boundTimelineTarget = null;
                    }

                    _avatarRegistry.Unregister(widget);

                    if (ReferenceEquals(abilitySystem?.GetAvatar<AdventureMonsterCardWidget>(), widget))
                    {
                        abilitySystem.ClearAvatar();
                    }

                    widget.Unbind();
                });
        }
    }
}
