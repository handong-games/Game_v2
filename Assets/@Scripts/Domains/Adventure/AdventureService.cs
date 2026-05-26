using System;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Core.Utility;
using Game.Data;
using Game.Generated;

namespace Domains.Adventure
{
    [Dependency]
    public sealed class AdventureService : IDisposable
    {
        public AdventureSession CurrentAdventure { get; private set; }

        public AdventureSession StartNew(ECharacter character)
        {
            DBManager.Instance.Character.Get(character);

            AdventureModel adventure = DBManager.Instance.Adventure.Get(EAdventure.Default);
            CurrentAdventure = new AdventureSession(
                character,
                adventure.Id,
                adventure.CardDeckId,
                adventure.MaxStageCount,
                RandomUtility.CreateSeed());

            return CurrentAdventure;
        }

        public AdventureStageDto GetCurrentStage()
        {
            AdventureModel adventure = DBManager.Instance.Adventure.Get(CurrentAdventure.AdventureId);

            if (CurrentAdventure.StageNumber == 1)
            {
                return new AdventureStageDto(EAdventureStageType.First, adventure.StartDrawCount);
            }

            if (CurrentAdventure.StageNumber >= CurrentAdventure.MaxStageCount)
            {
                return new AdventureStageDto(EAdventureStageType.Boss, 1);
            }

            return new AdventureStageDto(EAdventureStageType.Choice, adventure.StartDrawCount);
        }

        public EAdventureStageType GetCurrentStageType()
        {
            return GetCurrentStage().StageType;
        }

        public void AdvanceStage()
        {
            CurrentAdventure.AdvanceStage();
        }

        public void Dispose()
        {
            CurrentAdventure = null;
        }
    }
}
