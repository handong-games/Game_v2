using Game.Generated;

namespace Domains.Adventure
{
    public sealed class AdventureSession
    {
        public AdventureSession(ECharacter character, EAdventure adventure, ECardDeck cardDeck)
        {
            SelectedCharacterId = character;
            AdventureId = adventure;
            CardDeckId = cardDeck;
        }

        public ECharacter SelectedCharacterId { get; }
        public EAdventure AdventureId { get; }
        public ECardDeck CardDeckId { get; }
    }
}
