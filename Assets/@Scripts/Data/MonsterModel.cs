using System.Collections.Generic;
using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Monster")]
    public sealed class MonsterModel : CardModel<EMonster>, ICombatModel
    {
        private IReadOnlyList<GameplayTagReference> _runtimeOwnedTags;

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
        public override IReadOnlyList<GameplayTagReference> OwnedTags => _runtimeOwnedTags ??=
            CardGameplayTags.Combine(
                base.OwnedTags,
                CardGameplayTags.KindMonsterName,
                CardGameplayTags.CombatantName,
                CardGameplayTags.TargetableName,
                CardGameplayTags.GetMonsterRankName(_rank));
        public int MaxHp => _maxHp;
        public EMonsterRank Rank => _rank;
    }
}
