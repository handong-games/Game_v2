using Domains.Intent.Flow;

namespace Domains.Adventure
{
    // Role:
    // Starts combat from a selected offer card.
    // It does not replace cards because offer cards are already actual encounter cards.
    public sealed class AdventureCombatEncounterFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventurePlayer _player;
        private readonly AdventureCombatRuntime _combat;
        private readonly IntentExecutionSetupFlow _intentExecutionSetupFlow;

        public AdventureCombatEncounterFlow(
            AdventureProgress progress,
            AdventurePlayer player,
            AdventureCombatRuntime combat,
            IntentExecutionSetupFlow intentExecutionSetupFlow)
        {
            _progress = progress;
            _player = player;
            _combat = combat;
            _intentExecutionSetupFlow = intentExecutionSetupFlow;
        }

        public AdventureEncounterStartResult Start(AdventureChoiceCommitResult result)
        {
            _combat.InitializeCombat(_player.PlayerCard.CardId, result.OfferCardId);
            _intentExecutionSetupFlow.Setup(result.OfferCardId);
            _progress.EnterCombat();
            return new AdventureEncounterStartResult(result.OfferCardId, result.EncounterType);
        }
    }
}
