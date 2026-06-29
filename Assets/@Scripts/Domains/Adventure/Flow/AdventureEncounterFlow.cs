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
            _combatFlow = combatFlow ?? throw new ArgumentNullException(nameof(combatFlow));
            _eventFlow = eventFlow ?? throw new ArgumentNullException(nameof(eventFlow));
            _shopFlow = shopFlow ?? throw new ArgumentNullException(nameof(shopFlow));
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

        public void CompleteNonCombatEncounter(AdventureEncounterStartResult result)
        {
            switch (result.EncounterType)
            {
                case AdventureEncounterType.Event:
                    _eventFlow.Complete(result.OfferCardId);
                    break;
                case AdventureEncounterType.Shop:
                    _shopFlow.Complete(result.OfferCardId);
                    break;
                case AdventureEncounterType.Combat:
                case AdventureEncounterType.Boss:
                    throw new InvalidOperationException(
                        $"{result.EncounterType} must be completed by combat result flow.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(result.EncounterType), result.EncounterType, null);
            }
        }
    }
}
