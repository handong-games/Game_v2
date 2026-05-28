using System.Collections.Generic;
using Game.Generated;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character")]
    public sealed class CharacterModel : CardModel<ECharacter>
    {
        private IReadOnlyList<GameplayTag> _runtimeOwnedTags;

        [Header("Visual")]
        [SerializeField]
        private LocalizedString _localizedName;

        public LocalizedString LocalizedName => _localizedName;
        public override IReadOnlyList<GameplayTag> OwnedTags => _runtimeOwnedTags ??=
            CardGameplayTags.Combine(
                base.OwnedTags,
                CardGameplayTags.KindPlayer,
                CardGameplayTags.Combatant,
                CardGameplayTags.Targetable);
    }
}
