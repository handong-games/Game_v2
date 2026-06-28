using Game.Data;

namespace Domains.Adventure
{
    public readonly struct AdventureChoiceSelectionResult
    {
        public static readonly AdventureChoiceSelectionResult None = new(false, 0);

        public AdventureChoiceSelectionResult(bool selected, uint offerCardId)
        {
            Selected = selected;
            OfferCardId = offerCardId;
        }

        public bool Selected { get; }
        public uint OfferCardId { get; }
    }

    public readonly struct AdventureChoiceCommitResult
    {
        public AdventureChoiceCommitResult(
            uint offerCardId,
            AdventureEncounterType encounterType,
            CardModelBase encounterCard)
        {
            OfferCardId = offerCardId;
            EncounterType = encounterType;
            EncounterCard = encounterCard;
        }

        public uint OfferCardId { get; }
        public AdventureEncounterType EncounterType { get; }
        public CardModelBase EncounterCard { get; }
    }

    public readonly struct AdventureEncounterStartResult
    {
        public AdventureEncounterStartResult(
            uint offerCardId,
            AdventureEncounterType encounterType)
        {
            OfferCardId = offerCardId;
            EncounterType = encounterType;
        }

        public uint OfferCardId { get; }
        public AdventureEncounterType EncounterType { get; }
    }
}
