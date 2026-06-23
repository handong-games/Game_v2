using System;
using Domains.Scene.Adventure;
using Game.Data;

namespace Domains.Adventure
{
    public sealed class AdventureService : IDisposable
    {
        private readonly AdventureRunState _state;
        private readonly AdventureSceneInitialData _initialData;

        public AdventureService(AdventureRunState state, AdventureSceneInitialData initialData)
        {
            _state = state;
            _initialData = initialData;
        }

        public AdventureRun CurrentRun => _state.CurrentRun;

        public void SetCurrent(AdventureRun run)
        {
            _state.SetCurrent(run);
        }

        public AdventureStageDto GetCurrentStage()
        {
            AdventureRun currentRun = CurrentRun;

            if (currentRun.StageNumber == 1)
            {
                return new AdventureStageDto(EAdventureStageType.First, _initialData.Adventure.StartDrawCount);
            }

            if (currentRun.StageNumber >= currentRun.MaxStageCount)
            {
                return new AdventureStageDto(EAdventureStageType.Boss, 1);
            }

            return new AdventureStageDto(EAdventureStageType.Choice, _initialData.Adventure.StartDrawCount);
        }

        public EAdventureStageType GetCurrentStageType()
        {
            return GetCurrentStage().StageType;
        }

        public void AdvanceStage()
        {
            CurrentRun.AdvanceStage();
        }

        public void Dispose()
        {
            _state.Clear();
        }
    }
}
