using System.Collections.Generic;
using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character")]
    public sealed class CharacterModel : AbstractModel<ECharacter>, ICardModel
    {
        [SerializeField]
        private LocalizedString _localizedName;

        [SerializeField]
        private Sprite _portrait;

        [SerializeField]
        private int _coinCount;

        [SerializeField]
        private int _maxHp;

        [SerializeField]
        private int _defaultSkillSlotCount = 4;

        [SerializeField]
        private int _maxSkillSlotCount = 8;

        [SerializeField]
        private CharacterSkillModel[] _defaultSkills;

        public LocalizedString LocalizedName => _localizedName;
        public Sprite Portrait => _portrait;
        public int CoinCount => _coinCount;
        public int MaxHp => _maxHp;
        public int DefaultSkillSlotCount => _defaultSkillSlotCount;
        public int MaxSkillSlotCount => _maxSkillSlotCount;
        public IReadOnlyList<CharacterSkillModel> DefaultSkills => _defaultSkills;
    }
}
