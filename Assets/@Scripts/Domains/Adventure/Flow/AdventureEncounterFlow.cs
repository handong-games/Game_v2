using System;

namespace Domains.Adventure
{
    // Role:
    // Branches a committed encounter choice to the proper concrete encounter flow.
    // It executes the selected flow directly and does not return flow objects.
    public sealed class AdventureEncounterFlow
    {
        private readonly AdventureCombatEncounterFlow _combatFlow;
        private readonly AdventureEventFlow _eventFlow;
        private readonly AdventureShopFlow _shopFlow;

        public AdventureEncounterFlow(
            AdventureCombatEncounterFlow combatFlow,
            AdventureEventFlow eventFlow,
            AdventureShopFlow shopFlow)
        {
            _combatFlow = combatFlow;
            _eventFlow = eventFlow;
            _shopFlow = shopFlow;
        }

        public AdventureEncounterStartResult StartEncounter(AdventureChoiceCommitResult result)
        {
            return result.EncounterType switch
            {
                AdventureEncounterType.Combat => _combatFlow.Start(result),
                AdventureEncounterType.Boss => _combatFlow.Start(result),
                AdventureEncounterType.Event => _eventFlow.Start(result),
                AdventureEncounterType.Shop => _shopFlow.Start(result),
                _ => throw new ArgumentOutOfRangeException(nameof(result.EncounterType)),
            };
        }
    }
}
