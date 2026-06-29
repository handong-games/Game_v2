using System;
using Domains.Adventure;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Presents the next choice board after reward completion.
    // It owns choice-refresh timing and board visual replacement, not gameplay stage advancement.
    public sealed class AdventureChoiceRefreshUIFlow
    {
        private readonly AdventureBoardUIFlow _boardUIFlow;

        public AdventureChoiceRefreshUIFlow(AdventureBoardUIFlow boardUIFlow)
        {
            _boardUIFlow = boardUIFlow ?? throw new ArgumentNullException(nameof(boardUIFlow));
        }

        public async Awaitable<bool> Play(
            VisualElement cardDeck,
            AdventureChoiceRefreshViewModel viewModel,
            Func<bool> canContinue)
        {
            if (cardDeck == null)
                throw new ArgumentNullException(nameof(cardDeck));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel.Board == null)
                throw new InvalidOperationException("Choice refresh board presentation is missing.");

            await _boardUIFlow.RefreshRightSideFromDeckAfterExit(
                viewModel.Board.DisplayCards,
                cardDeck,
                canContinue);
            if (canContinue != null && !canContinue.Invoke())
                return false;

            return canContinue == null || canContinue.Invoke();
        }
    }
}
