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
        private BasicCardModel _monsterChoiceCard;

        [SerializeField]
        private BasicCardModel _eventChoiceCard;

        [SerializeField]
        private BasicCardModel _shopChoiceCard;

        [SerializeField]
        private BasicCardModel _bossChoiceCard;

        [SerializeField]
        private uint _monsterCardCount;

        [SerializeField]
        private uint _eventCardCount;

        [SerializeField]
        private uint _shopCardCount;

        public MonsterModel[] MonsterPool => _monsterPool;
        public EventModel[] EventPool => _eventPool;
        public ShopModel Shop => _shop;
        public MonsterModel Boss => _boss;
        public BasicCardModel MonsterChoiceCard => _monsterChoiceCard;
        public BasicCardModel EventChoiceCard => _eventChoiceCard;
        public BasicCardModel ShopChoiceCard => _shopChoiceCard;
        public BasicCardModel BossChoiceCard => _bossChoiceCard;
        public uint MonsterCardCount => _monsterCardCount;
        public uint EventCardCount => _eventCardCount;
        public uint ShopCardCount => _shopCardCount;
    }
}
