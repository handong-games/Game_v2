namespace Domains.Adventure
{
    public readonly struct AdventureStageRequest
    {
        public AdventureStageRequest(EAdventureStageType stageType, uint drawCount)
        {
            StageType = stageType;
            DrawCount = drawCount;
        }

        public EAdventureStageType StageType { get; }
        public uint DrawCount { get; }
    }
}
