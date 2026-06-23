using Game.Data;
using Game.Generated;

namespace Domains.Scene.Adventure
{
    public sealed class AdventureSceneInitialData
    {
        public AdventureSceneInitialData(
            ECharacter selectedCharacterId,
            CharacterModel character,
            AdventureModel adventure,
            CardDeckModel cardDeck,
            uint seed)
        {
            SelectedCharacterId = selectedCharacterId;
            Character = character;
            Adventure = adventure;
            CardDeck = cardDeck;
            Seed = seed;
        }

        public ECharacter SelectedCharacterId { get; }
        public CharacterModel Character { get; }
        public AdventureModel Adventure { get; }
        public CardDeckModel CardDeck { get; }
        public uint Seed { get; }
    }
}
