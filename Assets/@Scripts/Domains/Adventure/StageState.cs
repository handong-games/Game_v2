using System.Collections.Generic;
using System.Linq;
using Game.Data;
using Game.Generated;

namespace Domains.Adventure
{
    public sealed class StageState
    {
        private readonly List<EMonster> _remainingMonsterIds;
        private readonly List<EEvent> _remainingEventIds;

        public StageState(StageModel model)
        {
            Model = model;
            StageNumber = 1;

            _remainingMonsterIds = model.MonsterPool
                .Select(monster => monster.Id)
                .ToList();

            _remainingEventIds = model.EventPool
                .Select(stageEvent => stageEvent.Id)
                .ToList();

            ShopId = model.Shop.Id;
            BossId = model.Boss.Id;
        }

        public StageModel Model { get; }

        public EStage Id => Model.Id;
        public int StageNumber { get; private set; }

        public IReadOnlyList<EMonster> RemainingMonsterIds => _remainingMonsterIds;
        public IReadOnlyList<EEvent> RemainingEventIds => _remainingEventIds;

        public EShop ShopId { get; }
        public EMonster BossId { get; }

        public void AdvanceStage()
        {
            StageNumber++;
        }

        public void RemoveMonster(EMonster monsterId)
        {
            _remainingMonsterIds.Remove(monsterId);
        }

        public void RemoveEvent(EEvent eventId)
        {
            _remainingEventIds.Remove(eventId);
        }
    }
}
