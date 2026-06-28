using Domains.Card;

namespace Domains.Adventure
{
    // Role:
    // Starts and completes Shop encounters.
    // It does not open UI panels directly.
    public sealed class AdventureShopFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventureBoard _board;
        private readonly AdventureRunState _runState;

        public AdventureShopFlow(
            AdventureProgress progress,
            AdventureBoard board,
            AdventureRunState runState)
        {
            _progress = progress;
            _board = board;
            _runState = runState;
        }

        public AdventureEncounterStartResult Start(AdventureChoiceCommitResult result)
        {
            _progress.EnterShop();
            return new AdventureEncounterStartResult(result.OfferCardId, result.EncounterType);
        }

        public void Complete(uint offerCardId)
        {
            _board.RemoveCard(ECardZone.Right, offerCardId);
            _runState.CurrentRun?.AdvanceStage();
        }
    }
}
