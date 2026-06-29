using Domains.Adventure;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Plays the first Adventure screen presentation sequence.
    // It owns intro presentation timing but does not mutate Adventure runtime state.
    public sealed class AdventureIntroUIFlow
    {
        private const string AdventureIntroShownClass = "adventure-view--intro-shown";

        private readonly AdventureBoardUIFlow _boardUIFlow;
        private readonly AdventureResourceStatusUIFlow _resourceStatusUIFlow;
        private readonly ViewTransitionManager _viewTransitionManager;

        public AdventureIntroUIFlow(
            AdventureBoardUIFlow boardUIFlow,
            AdventureResourceStatusUIFlow resourceStatusUIFlow,
            ViewTransitionManager viewTransitionManager)
        {
            _boardUIFlow = boardUIFlow ?? throw new ArgumentNullException(nameof(boardUIFlow));
            _resourceStatusUIFlow = resourceStatusUIFlow ?? throw new ArgumentNullException(nameof(resourceStatusUIFlow));
            _viewTransitionManager = viewTransitionManager ?? throw new ArgumentNullException(nameof(viewTransitionManager));
        }

        public async Awaitable Play(
            VisualElement adventureRoot,
            VisualElement background,
            VisualElement emblem,
            VisualElement cardDeck,
            ResourceStatusBar resourceStatusBar,
            Banner banner,
            AdventureInitialPresentationViewModel viewModel,
            System.Func<bool> canContinue)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (adventureRoot == null)
                throw new ArgumentNullException(nameof(adventureRoot));

            if (background == null)
                throw new ArgumentNullException(nameof(background));

            if (emblem == null)
                throw new ArgumentNullException(nameof(emblem));

            if (cardDeck == null)
                throw new ArgumentNullException(nameof(cardDeck));

            if (resourceStatusBar == null)
                throw new ArgumentNullException(nameof(resourceStatusBar));

            if (banner == null)
                throw new ArgumentNullException(nameof(banner));

            adventureRoot.RemoveFromClassList(AdventureIntroShownClass);
            ApplyBackground(background, viewModel.Entry.Background);
            ApplyOptionalSprite(emblem, viewModel.Entry.Emblem);

            _resourceStatusUIFlow.Prepare(resourceStatusBar, viewModel.ResourceStatus);

            await Awaitable.NextFrameAsync();
            if (canContinue != null && !canContinue.Invoke())
                return;

            Awaitable introCompletion = WaitForIntroCompletion(cardDeck);
            Awaitable bannerCompletion = banner.PresentRegion(
                viewModel.Entry.Subtitle,
                viewModel.Entry.Title,
                _viewTransitionManager);
            adventureRoot.AddToClassList(AdventureIntroShownClass);

            await introCompletion;
            if (canContinue != null && !canContinue.Invoke())
                return;

            Awaitable<AdventureBoardPlacementResult> boardEnterCompletion =
                _boardUIFlow.ReplaceBoardWithEnter(viewModel.BoardCards, cardDeck, canContinue);

            await boardEnterCompletion;
            await bannerCompletion;
        }

        private static void ApplyBackground(VisualElement background, Sprite sprite)
        {
            if (background == null || sprite == null)
                return;

            background.style.backgroundImage =
                new StyleBackground(Background.FromSprite(sprite));
        }

        private static void ApplyOptionalSprite(VisualElement element, Sprite sprite)
        {
            if (element == null)
                return;

            if (sprite == null)
            {
                element.style.display = DisplayStyle.None;
                element.style.backgroundImage = StyleKeyword.Null;
                return;
            }

            element.style.display = DisplayStyle.Flex;
            element.style.backgroundImage =
                new StyleBackground(Background.FromSprite(sprite));
        }

        private static Awaitable WaitForIntroCompletion(VisualElement cardDeck)
        {
            if (cardDeck == null || cardDeck.panel == null)
                return Awaitable.NextFrameAsync();

            return ViewTransitionAwaiter.WaitForEnd(cardDeck);
        }
    }
}
