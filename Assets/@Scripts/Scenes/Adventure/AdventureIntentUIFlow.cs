using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.Intent.Presentation;
using Domains.View.Widgets;
using UnityEngine;

namespace Game.Scenes.Adventure
{
    // Role:
    // Plays intent badge presentation for monster cards already placed on the Adventure board.
    // It does not decide monster intent or execute combat actions.
    public sealed class AdventureIntentUIFlow
    {
        private readonly AdventureBoardUIFlow _boardUIFlow;

        public AdventureIntentUIFlow(AdventureBoardUIFlow boardUIFlow)
        {
            _boardUIFlow = boardUIFlow ?? throw new ArgumentNullException(nameof(boardUIFlow));
        }

        public async Awaitable PlayReveal(
            IReadOnlyList<MonsterIntentRevealViewModel> revealSequence,
            Func<bool> canContinue)
        {
            if (revealSequence == null)
                throw new ArgumentNullException(nameof(revealSequence));

            if (canContinue != null && !canContinue.Invoke())
                return;

            for (int i = 0; i < revealSequence.Count; i++)
            {
                MonsterIntentRevealViewModel viewModel = revealSequence[i];
                if (viewModel == null)
                    throw new InvalidOperationException($"Intent reveal sequence contains null item at index {i}.");

                if (!TryFindIntentCard(viewModel.CardId, out IAdventureIntentCardWidget widget))
                {
                    throw new InvalidOperationException(
                        $"Intent card widget is missing for card id {viewModel.CardId}.");
                }

                await widget.ShowIntentAsync(viewModel.Items);
                if (canContinue != null && !canContinue.Invoke())
                    return;
            }
        }

        public async Awaitable<bool> PlayTriggered(uint cardId, Func<bool> canContinue)
        {
            if (canContinue != null && !canContinue.Invoke())
                return false;

            if (!TryFindIntentCard(cardId, out IAdventureIntentCardWidget widget))
                throw new InvalidOperationException($"Intent card widget is missing for card id {cardId}.");

            await widget.TriggerIntentAsync();
            return canContinue == null || canContinue.Invoke();
        }

        public async Awaitable PlayRefresh(
            MonsterIntentRevealViewModel viewModel,
            Func<bool> canContinue)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (canContinue != null && !canContinue.Invoke())
                return;

            if (!TryFindIntentCard(viewModel.CardId, out IAdventureIntentCardWidget widget))
            {
                throw new InvalidOperationException(
                    $"Intent card widget is missing for card id {viewModel.CardId}.");
            }

            await widget.RefreshIntentAsync(viewModel.Items);
        }

        private bool TryFindIntentCard(
            uint cardId,
            out IAdventureIntentCardWidget widget)
        {
            return _boardUIFlow.TryGetCardWidget(
                AdventureBoardSide.Right,
                cardId,
                out widget);
        }
    }
}
