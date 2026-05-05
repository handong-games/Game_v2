using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Card Deck")]
    public sealed class CardDeckModel : AbstractModel<ECardDeck>
    {
        [SerializeField]
        private MonsterModel[] _monsterPool;

        [SerializeField]
        private EventModel[] _eventPool;

        [SerializeField]
        private ShopModel _shop;

        [SerializeField]
        private MonsterModel _boss;

        [SerializeField]
        private uint _monsterCount;

        [SerializeField]
        private uint _eventCount;

        [SerializeField]
        private uint _shopCount;

        public MonsterModel[] MonsterPool => _monsterPool;
        public EventModel[] EventPool => _eventPool;
        public ShopModel Shop => _shop;
        public MonsterModel Boss => _boss;
        public uint MonsterCount => _monsterCount;
        public uint EventCount => _eventCount;
        public uint ShopCount => _shopCount;
    }
}
