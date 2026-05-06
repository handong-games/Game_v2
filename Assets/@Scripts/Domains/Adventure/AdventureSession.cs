using Game.Generated;

namespace Domains.Adventure
{
    public sealed class AdventureSession
    {
        public AdventureSession(ECharacter character, EAdventure adventure, ECardDeck cardDeck, uint seed)
        {
            SelectedCharacterId = character;
            AdventureId = adventure;
            CardDeckId = cardDeck;
            Seed = seed;
        }

        public ECharacter SelectedCharacterId { get; }
        public EAdventure AdventureId { get; }
        public ECardDeck CardDeckId { get; }
        public uint Seed { get; }
    }
}
