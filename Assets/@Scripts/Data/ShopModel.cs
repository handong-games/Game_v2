using System.Collections.Generic;
using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Shop")]
    public sealed class ShopModel : CardModel<EShop>
    {
        private IReadOnlyList<GameplayTagReference> _runtimeOwnedTags;

        public override IReadOnlyList<GameplayTagReference> OwnedTags => _runtimeOwnedTags ??=
            CardGameplayTags.Combine(base.OwnedTags, CardGameplayTags.KindShopName);
    }
}
