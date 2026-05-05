using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Adventure Table")]
    public sealed class AdventureTable : AbstractTable<AdventureModel, EAdventure>
    {
        public AdventureModel Get(EAdventure key)
        {
            return Get((int)key);
        }
    }
}
