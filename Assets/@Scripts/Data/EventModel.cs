using System.Collections.Generic;
using Game.Generated;
using Gameplay.GAS;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Event")]
    public sealed class EventModel : CardModel<EEvent>
    {
        private IReadOnlyList<GameplayTag> _runtimeOwnedTags;

        public override IReadOnlyList<GameplayTag> OwnedTags => _runtimeOwnedTags ??=
            CardGameplayTags.Combine(base.OwnedTags, CardGameplayTags.KindEvent);
    }
}
