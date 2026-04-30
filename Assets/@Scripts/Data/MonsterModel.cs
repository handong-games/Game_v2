using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Monster")]
    public sealed class MonsterModel : AbstractModel<EMonster>
    {
        [SerializeField]
        private LocalizedString _localizedName;

        [SerializeField]
        private Sprite _portrait;

        [SerializeField]
        private int _maxHp;

        [SerializeField]
        private EMonsterRank _rank;

        public LocalizedString LocalizedName => _localizedName;
        public Sprite Portrait => _portrait;
        public int MaxHp => _maxHp;
        public EMonsterRank Rank => _rank;
    }
}
