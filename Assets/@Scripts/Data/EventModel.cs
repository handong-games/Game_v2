using System.Collections.Generic;
using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Event")]
    public sealed class EventModel : CardModel<EEvent>
    {
        private IReadOnlyList<GameplayTagReference> _runtimeOwnedTags;

        public override IReadOnlyList<GameplayTagReference> OwnedTags => _runtimeOwnedTags ??=
            CardGameplayTags.Combine(base.OwnedTags, CardGameplayTags.KindEventName);
    }
}
