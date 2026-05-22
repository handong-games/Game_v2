using Game.Generated;

namespace Domains.Adventure
{
    public sealed class AdventureSession
    {
        public AdventureSession(ECharacter character, EAdventure adventure, ECardDeck cardDeck, uint maxStageCount, uint seed)
        {
            SelectedCharacterId = character;
            AdventureId = adventure;
            CardDeckId = cardDeck;
            MaxStageCount = maxStageCount;
            Seed = seed;
            StageNumber = 1;
        }

        public ECharacter SelectedCharacterId { get; }
        public EAdventure AdventureId { get; }
        public ECardDeck CardDeckId { get; }
        public uint MaxStageCount { get; }
        public uint Seed { get; }
        public uint StageNumber { get; private set; }

        public void AdvanceStage()
        {
            StageNumber++;
        }
    }
}
