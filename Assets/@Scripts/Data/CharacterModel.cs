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
        private CharacterSkillModel[] _defaultSkills;

        public LocalizedString LocalizedName => _localizedName;
        public Sprite Portrait => _portrait;
        public int CoinCount => _coinCount;
        public int MaxHp => _maxHp;
        public IReadOnlyList<CharacterSkillModel> DefaultSkills => _defaultSkills;
    }
}
