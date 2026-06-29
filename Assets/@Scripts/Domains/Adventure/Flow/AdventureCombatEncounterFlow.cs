using Domains.Intent.Flow;
using Domains.Intent.Execution;
using Domains.Intent.Runtime;
using System;

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
        private readonly IntentRuntime _intentRuntime;
        private readonly ActionExecutionBindingStore _actionBindings;
        private readonly IntentExecutionSetupFlow _intentExecutionSetupFlow;

        public AdventureCombatEncounterFlow(
            AdventureProgress progress,
            AdventurePlayer player,
            AdventureCombatRuntime combat,
            IntentRuntime intentRuntime,
            ActionExecutionBindingStore actionBindings,
            IntentExecutionSetupFlow intentExecutionSetupFlow)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _intentRuntime = intentRuntime ?? throw new ArgumentNullException(nameof(intentRuntime));
            _actionBindings = actionBindings ?? throw new ArgumentNullException(nameof(actionBindings));
            _intentExecutionSetupFlow = intentExecutionSetupFlow ?? throw new ArgumentNullException(nameof(intentExecutionSetupFlow));
        }

        public AdventureEncounterStartResult Start(AdventureChoiceCommitResult result)
        {
            _intentRuntime.Clear();
            _actionBindings.Clear();
            _combat.InitializeCombat(_player.PlayerCard.CardId, result.OfferCardId);
            _intentExecutionSetupFlow.Setup(result.OfferCardId);
            _progress.EnterCombat();
            return new AdventureEncounterStartResult(result.OfferCardId, result.EncounterType);
        }
    }
}
