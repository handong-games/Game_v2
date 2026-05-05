using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Adventure")]
    public sealed class AdventureModel : AbstractModel<EAdventure>
    {
        [SerializeField]
        private LocalizedString _localizedRegionName;

        [SerializeField]
        private ECardDeck _cardDeckId;

        [SerializeField]
        private uint _startDrawCount = 2;

        public LocalizedString LocalizedRegionName => _localizedRegionName;
        public ECardDeck CardDeckId => _cardDeckId;
        public uint StartDrawCount => _startDrawCount == 0 ? 1 : _startDrawCount;
    }
}
