using Game.Data;

namespace Domains.Adventure
{
    // Role:
    // Carries the game meaning needed to create one choice card in the adventure board UI.
    // OfferCardId is the runtime click id for this choice, not a UI-only temporary id.
    public sealed class AdventureChoiceCardViewModel : AdventureBoardCardViewModel
    {
        public AdventureChoiceCardViewModel(
            uint offerCardId,
            EChoiceCardType choiceType)
            : base(AdventureBoardSide.Right, offerCardId, null)
        {
            OfferCardId = offerCardId;
            ChoiceType = choiceType;
        }

        public uint OfferCardId { get; }
        public EChoiceCardType ChoiceType { get; }
    }
}
