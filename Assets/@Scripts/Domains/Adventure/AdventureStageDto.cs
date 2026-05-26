namespace Domains.Adventure
{
    public readonly struct AdventureStageDto
    {
        public AdventureStageDto(EAdventureStageType stageType, uint drawCount)
        {
            StageType = stageType;
            DrawCount = drawCount;
        }

        public EAdventureStageType StageType { get; }
        public uint DrawCount { get; }
    }
}
