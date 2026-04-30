using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Stage")]
    public sealed class StageModel : AbstractModel<EStage>
    {
        [SerializeField]
        private MonsterModel[] _monsterPool;

        [SerializeField]
        private EventModel[] _eventPool;

        [SerializeField]
        private ShopModel _shop;

        [SerializeField]
        private MonsterModel _boss;

        public MonsterModel[] MonsterPool => _monsterPool;
        public EventModel[] EventPool => _eventPool;
        public ShopModel Shop => _shop;
        public MonsterModel Boss => _boss;
    }
}
