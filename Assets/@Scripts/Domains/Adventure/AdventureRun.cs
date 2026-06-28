using Game.Generated;

namespace Domains.Adventure
{
    public sealed class AdventureRun
    {
        public AdventureRun(ECharacter character, EAdventure adventure, uint maxStageCount, uint seed)
        {
            SelectedCharacterId = character;
            AdventureId = adventure;
            MaxStageCount = maxStageCount;
            Seed = seed;
            StageNumber = 1;
        }

        public ECharacter SelectedCharacterId { get; }
        public EAdventure AdventureId { get; }
        public uint MaxStageCount { get; }
        public uint Seed { get; }
        public uint StageNumber { get; private set; }

        public void AdvanceStage()
        {
            StageNumber++;
        }
    }
}
