using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class CardDealer
    {
        private const float StartScale = 0.28f;
        private const float DealFallbackSeconds = 0.8f;
        private const string CardDealEnterClass = "card-deal--enter";

        private static void PrepareCardBeforeLayout(VisualElement card)
        {
            card.style.opacity = 0f;
            card.style.scale = new Scale(new Vector2(StartScale, StartScale));
        }

        private void PrepareCardFromDeck(VisualElement card)
        {
            Vector2 deckCenter = _cardDeck.worldBound.center;
            Vector2 cardCenter = card.worldBound.center;
            Vector2 offset = deckCenter - cardCenter;

            card.RemoveFromClassList(CardDealEnterClass);
            card.style.opacity = 0f;
            card.style.scale = new Scale(new Vector2(StartScale, StartScale));
            card.style.translate = new Translate(
                new Length(offset.x, LengthUnit.Pixel),
                new Length(offset.y, LengthUnit.Pixel));
        }

        private async Awaitable PlayDealEnterAsync(VisualElement card)
        {
            AwaitableCompletionSource completionSource = new();
            bool completed = false;

            EventCallback<TransitionEndEvent> onTransitionEnd = evt =>
            {
                if (evt.target != card || completed)
                    return;

                completed = true;
                completionSource.SetResult();
            };

            EventCallback<TransitionCancelEvent> onTransitionCancel = evt =>
            {
                if (evt.target != card || completed)
                    return;

                completed = true;
                completionSource.SetResult();
            };

            card.RegisterCallback(onTransitionEnd);
            card.RegisterCallback(onTransitionCancel);
            _ = CompleteDealAfterFallback(() =>
            {
                if (completed)
                    return;

                completed = true;
                completionSource.SetResult();
            });

            await Awaitable.NextFrameAsync();

            card.AddToClassList(CardDealEnterClass);

            await Awaitable.NextFrameAsync();

            ClearDealStartStyle(card);

            await completionSource.Awaitable;

            card.UnregisterCallback(onTransitionEnd);
            card.UnregisterCallback(onTransitionCancel);
        }

        private static async Awaitable CompleteDealAfterFallback(Action complete)
        {
            await Awaitable.WaitForSecondsAsync(DealFallbackSeconds);
            complete();
        }

        private static void ClearDealStartStyle(VisualElement card)
        {
            card.style.opacity = StyleKeyword.Null;
            card.style.scale = StyleKeyword.Null;
            card.style.translate = StyleKeyword.Null;
        }
    }
}
