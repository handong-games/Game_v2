using Domains.Adventure;
using Game.Data;
using System;
using UnityEngine.UIElements;
using CardActor = Domains.Card.Card;

namespace Domains.View.Widgets
{
    // Role:
    // Creates concrete adventure card widgets from board card presentation data.
    // Board placement stays outside this factory.
    public sealed class AdventureCardWidgetFactory
    {
        private readonly AdventureChoiceCardUIModels _choiceCardUIModels;
        private readonly AdventureCards _cards;
        private readonly AdventureCardWidgetTemplates _templates;

        public AdventureCardWidgetFactory(
            AdventureChoiceCardUIModels choiceCardUIModels,
            AdventureCards cards,
            AdventureCardWidgetTemplates templates)
        {
            _choiceCardUIModels = choiceCardUIModels ?? throw new ArgumentNullException(nameof(choiceCardUIModels));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        public AdventureBoardCardWidgetBinding Create(
            AdventureBoardCardViewModel viewModel)
        {
            return viewModel switch
            {
                AdventureChoiceCardViewModel choice => CreateChoiceCard(choice),
                AdventureBoardCardViewModel boardCard => CreateBoardCard(boardCard),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel), viewModel, null),
            };
        }

        public AdventureChoiceCardWidget CreateChoiceCard(
            AdventureChoiceCardViewModel viewModel,
            AdventureChoiceCardUIModel uiModel)
        {
            AdventureChoiceCardWidget widget = new(_templates.ChoiceCard);
            widget.Bind(viewModel, uiModel);
            return widget;
        }

        private AdventureBoardCardWidgetBinding CreateChoiceCard(
            AdventureChoiceCardViewModel viewModel)
        {
            AdventureChoiceCardUIModel uiModel =
                _choiceCardUIModels.Get(viewModel.ChoiceType);

            AdventureChoiceCardWidget widget =
                CreateChoiceCard(viewModel, uiModel);

            return new AdventureBoardCardWidgetBinding(
                viewModel,
                widget,
                onPlaced: null,
                dispose: widget.Unbind,
                interactionTarget: widget.InteractionTarget);
        }

        private AdventureBoardCardWidgetBinding CreateBoardCard(
            AdventureBoardCardViewModel viewModel)
        {
            return viewModel.Side switch
            {
                AdventureBoardSide.Left => CreatePlayerCard(viewModel),
                AdventureBoardSide.Right => CreateRightSideCard(viewModel),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(viewModel.Side),
                    viewModel.Side,
                    null),
            };
        }

        private AdventureBoardCardWidgetBinding CreatePlayerCard(
            AdventureBoardCardViewModel viewModel)
        {
            if (!_cards.TryGet(viewModel.CardId, out CardActor cardActor))
            {
                throw new InvalidOperationException(
                    $"Adventure card runtime object is missing: {viewModel.CardId}");
            }

            AdventurePlayerCardWidget widget = AdventurePlayerCardWidget.Create(_templates.PlayerCard);
            widget.Bind(viewModel.Card, cardActor.AbilitySystem, _templates);

            return CreateRuntimeCardBinding(
                viewModel,
                widget,
                cardActor,
                widget.Unbind,
                widget.InteractionTarget);
        }

        private AdventureBoardCardWidgetBinding CreateRightSideCard(
            AdventureBoardCardViewModel viewModel)
        {
            if (!_cards.TryGet(viewModel.CardId, out CardActor cardActor))
            {
                throw new InvalidOperationException(
                    $"Adventure card runtime object is missing: {viewModel.CardId}");
            }

            if (cardActor.Model is MonsterModel)
                return CreateMonsterCard(viewModel, cardActor);

            return CreateDisplayCard(viewModel);
        }

        private AdventureBoardCardWidgetBinding CreateMonsterCard(
            AdventureBoardCardViewModel viewModel,
            CardActor cardActor)
        {
            AdventureMonsterCardWidget widget = AdventureMonsterCardWidget.Create(_templates.MonsterCard);
            widget.Bind(viewModel.Card, cardActor.AbilitySystem, _templates);

            return CreateRuntimeCardBinding(
                viewModel,
                widget,
                cardActor,
                widget.Unbind,
                widget.InteractionTarget);
        }

        private AdventureBoardCardWidgetBinding CreateDisplayCard(
            AdventureBoardCardViewModel viewModel)
        {
            if (viewModel.Card == null)
                throw new InvalidOperationException("Display card view model is missing card presentation data.");

            AdventureDisplayCardWidget widget = AdventureDisplayCardWidget.Create(_templates.DisplayCard);
            widget.Bind(viewModel.Card, _templates);

            return new AdventureBoardCardWidgetBinding(
                viewModel,
                widget,
                onPlaced: null,
                dispose: widget.Unbind,
                isInteractive: false,
                interactionTarget: widget.InteractionTarget);
        }

        private AdventureBoardCardWidgetBinding CreateRuntimeCardBinding(
            AdventureBoardCardViewModel viewModel,
            VisualElement widget,
            CardActor cardActor,
            System.Action unbind,
            VisualElement interactionTarget)
        {
            if (widget is not IAdventureHealthCardWidget)
            {
                throw new InvalidOperationException(
                    $"{widget.GetType().Name} must implement {nameof(IAdventureHealthCardWidget)}.");
            }

            VisualElement boundTimelineTarget = null;

            return new AdventureBoardCardWidgetBinding(
                viewModel,
                widget,
                onPlaced: placement =>
                {
                    if (cardActor.Timeline.IsBound)
                        throw new InvalidOperationException(
                            $"Card timeline is already bound before board placement: {cardActor.CardId}");

                    boundTimelineTarget = placement.Anchor;
                    cardActor.Timeline.Bind(boundTimelineTarget);
                },
                dispose: () =>
                {
                    if (cardActor.Timeline.IsBound && boundTimelineTarget != null)
                    {
                        cardActor.Timeline.Release(boundTimelineTarget);
                        boundTimelineTarget = null;
                    }

                    unbind.Invoke();
                },
                interactionTarget: interactionTarget);
        }
    }
}
