using System.Collections.Generic;
using System;

namespace Domains.Adventure
{
    // Role:
    // Carries board display cards and their matching runtime cards for one board presentation update.
    public sealed class AdventureBoardPresentationViewModel
    {
        public AdventureBoardPresentationViewModel(
            IReadOnlyList<AdventureBoardCardViewModel> displayCards,
            IReadOnlyList<AdventureCardViewModel> runtimeCards)
        {
            DisplayCards = displayCards ?? throw new ArgumentNullException(nameof(displayCards));
            RuntimeCards = runtimeCards ?? throw new ArgumentNullException(nameof(runtimeCards));
        }

        public IReadOnlyList<AdventureBoardCardViewModel> DisplayCards { get; }
        public IReadOnlyList<AdventureCardViewModel> RuntimeCards { get; }
    }

    // Role:
    // Carries one board card removal request and the board runtime state after removal.
    public sealed class AdventureBoardCardRemoveViewModel
    {
        public AdventureBoardCardRemoveViewModel(
            uint cardId,
            AdventureBoardPresentationViewModel board)
        {
            CardId = cardId;
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public uint CardId { get; }
        public AdventureBoardPresentationViewModel Board { get; }
    }

    // Role:
    // Carries one board card death request. Death presentation includes the card leaving the board.
    public sealed class AdventureBoardCardDeathViewModel
    {
        public AdventureBoardCardDeathViewModel(uint cardId)
        {
            CardId = cardId;
        }

        public uint CardId { get; }
    }

    // Role:
    // Carries an explicit request to clear one board side and the board runtime state after clearing.
    public sealed class AdventureBoardSideClearViewModel
    {
        public AdventureBoardSideClearViewModel(
            AdventureBoardSide side,
            AdventureBoardPresentationViewModel board)
        {
            Side = side;
            Board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public AdventureBoardSide Side { get; }
        public AdventureBoardPresentationViewModel Board { get; }
    }
}
