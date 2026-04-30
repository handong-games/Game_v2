using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Region")]
    public sealed class RegionModel : AbstractModel<ERegion>
    {
        [SerializeField]
        private LocalizedString _localizedName;

        [SerializeField]
        private EStage _stageId;

        [SerializeField]
        private uint _monsterCount;

        [SerializeField]
        private uint _eventCount;

        [SerializeField]
        private uint _shopCount;

        public LocalizedString LocalizedName => _localizedName;
        public EStage StageId => _stageId;
        public uint MonsterCount => _monsterCount;
        public uint EventCount => _eventCount;
        public uint ShopCount => _shopCount;
    }
}
