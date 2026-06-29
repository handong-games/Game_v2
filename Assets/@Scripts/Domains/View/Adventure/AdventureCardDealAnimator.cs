using System;
using System.Collections.Generic;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Plays the intro deal motion where real board card widgets enter from the visible card deck.
    public sealed class AdventureCardDealAnimator
    {
        private const float DeckEnterStartScale = 0.28f;
        private const float DeckNudgeDistance = 6f;
        private const string CardEnterPendingClass = "card-board__card-anchor--enter-pending";
        private const string CardDeckTopCardName = "card-deck-top-card";
        private const string CardDealEnterClass = "card-board__card--deal-enter";
        private const string CardDealSettleClass = "card-board__card--deal-settle";
        private const string CardDealSettleReleaseClass = "card-board__card--deal-settle-release";

        public async Awaitable Play(
            IReadOnlyList<AdventureBoardCardPlacement> placements,
            VisualElement cardDeck)
        {
            if (placements == null || placements.Count == 0)
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            if (cardDeck == null)
                throw new ArgumentNullException(nameof(cardDeck));

            try
            {
                for (int i = 0; i < placements.Count; i++)
                {
                    await Play(placements[i], cardDeck);
                }
            }
            finally
            {
                ClearDeckNudge(cardDeck);
            }
        }

        private async Awaitable Play(
            AdventureBoardCardPlacement placement,
            VisualElement cardDeck)
        {
            if (placement == null || placement.Anchor == null || placement.Card == null)
            {
                await Awaitable.NextFrameAsync();
                return;
            }

            VisualElement anchor = placement.Anchor;
            VisualElement card = GetDealTarget(placement.Card);
            await Awaitable.NextFrameAsync();

            if (anchor.panel == null || card.panel == null || cardDeck.panel == null)
                return;

            try
            {
                VisualElement topCard = cardDeck.Q<VisualElement>(CardDeckTopCardName) ?? cardDeck;
                PrepareCardFromDeck(card, topCard);
                NudgeTopCard(topCard, card);

                Awaitable dealTransition = ViewTransitionAwaiter.WaitForEnd(card);
                await Awaitable.NextFrameAsync();

                anchor.RemoveFromClassList(CardEnterPendingClass);
                card.AddToClassList(CardDealEnterClass);

                await Awaitable.NextFrameAsync();
                ClearDeckEnterStyle(card);

                await dealTransition;

                if (anchor.panel == null || card.panel == null || cardDeck.panel == null)
                    return;

                await PlaySettle(card);
            }
            finally
            {
                ClearDeckEnterStyle(card);
                ClearDealClasses(card);
            }
        }

        private static async Awaitable PlaySettle(VisualElement card)
        {
            Awaitable pressTransition = ViewTransitionAwaiter.WaitForEnd(card);
            card.AddToClassList(CardDealSettleClass);
            await pressTransition;

            if (card.panel == null)
                return;

            Awaitable releaseTransition = ViewTransitionAwaiter.WaitForEnd(card);
            card.RemoveFromClassList(CardDealSettleClass);
            card.AddToClassList(CardDealSettleReleaseClass);
            await releaseTransition;

            card.RemoveFromClassList(CardDealSettleReleaseClass);
        }

        private static void PrepareCardFromDeck(
            VisualElement card,
            VisualElement topCard)
        {
            Vector2 deckCenter = topCard.worldBound.center;
            Vector2 cardCenter = card.worldBound.center;
            Vector2 offset = deckCenter - cardCenter;

            card.RemoveFromClassList(CardDealEnterClass);
            card.RemoveFromClassList(CardDealSettleClass);
            card.RemoveFromClassList(CardDealSettleReleaseClass);
            card.style.opacity = 0f;
            card.style.scale = new Scale(new Vector2(DeckEnterStartScale, DeckEnterStartScale));
            card.style.translate = new Translate(
                new Length(offset.x, LengthUnit.Pixel),
                new Length(offset.y, LengthUnit.Pixel));
        }

        private static void NudgeTopCard(
            VisualElement topCard,
            VisualElement card)
        {
            Vector2 start = topCard.worldBound.center;
            Vector2 end = card.worldBound.center;
            Vector2 direction = end - start;
            if (direction.sqrMagnitude <= 0.001f)
                return;

            direction.Normalize();
            topCard.style.translate = new Translate(
                new Length(direction.x * DeckNudgeDistance, LengthUnit.Pixel),
                new Length(direction.y * DeckNudgeDistance, LengthUnit.Pixel));
            _ = ResetDeckNudge(topCard);
        }

        private static async Awaitable ResetDeckNudge(VisualElement topCard)
        {
            await Awaitable.WaitForSecondsAsync(0.08f);
            if (topCard?.panel == null)
                return;

            topCard.style.translate = StyleKeyword.Null;
        }

        private static void ClearDeckNudge(VisualElement cardDeck)
        {
            if (cardDeck == null)
                return;

            VisualElement topCard = cardDeck.Q<VisualElement>(CardDeckTopCardName);
            if (topCard != null)
                topCard.style.translate = StyleKeyword.Null;
        }

        private static void ClearDeckEnterStyle(VisualElement card)
        {
            if (card == null)
                return;

            card.style.opacity = StyleKeyword.Null;
            card.style.scale = StyleKeyword.Null;
            card.style.translate = StyleKeyword.Null;
        }

        private static void ClearDealClasses(VisualElement card)
        {
            if (card == null)
                return;

            card.RemoveFromClassList(CardDealEnterClass);
            card.RemoveFromClassList(CardDealSettleClass);
            card.RemoveFromClassList(CardDealSettleReleaseClass);
        }

        private static VisualElement GetDealTarget(VisualElement cardRoot)
        {
            if (cardRoot == null)
                return null;

            CardWidget cardWidget = cardRoot.Q<CardWidget>();
            if (cardWidget != null)
                return cardWidget;

            VisualElement baseCardRoot = cardRoot.Q<VisualElement>(null, "card-widget");
            return baseCardRoot ?? cardRoot;
        }
    }
}
