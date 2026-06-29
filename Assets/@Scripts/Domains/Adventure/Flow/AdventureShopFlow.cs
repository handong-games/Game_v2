using Domains.Card;
using System;

namespace Domains.Adventure
{
    // Role:
    // Starts and completes Shop encounters.
    // It detaches the selected card from board state, but registry removal is delayed until UI exit completes.
    public sealed class AdventureShopFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventureBoard _board;

        public AdventureShopFlow(
            AdventureProgress progress,
            AdventureBoard board)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public AdventureEncounterStartResult Start(AdventureChoiceCommitResult result)
        {
            _progress.EnterShop();
            return new AdventureEncounterStartResult(result.OfferCardId, result.EncounterType);
        }

        public void Complete(uint offerCardId)
        {
            _board.RemoveCard(ECardZone.Right, offerCardId);
        }
    }
}
