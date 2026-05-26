using Domains.Card;
using Gameplay.GAS;

namespace Domains.Adventure
{
    public sealed class AdventureCardViewModel
    {
        public AdventureCardViewModel(
            uint cardId,
            ECardZone zone,
            CardViewModel card,
            AbilitySystemComponent abilitySystem = null)
        {
            CardId = cardId;
            Zone = zone;
            Card = card;
            AbilitySystem = abilitySystem;
        }

        public uint CardId { get; }
        public ECardZone Zone { get; }
        public CardViewModel Card { get; }
        public AbilitySystemComponent AbilitySystem { get; }
    }
}
