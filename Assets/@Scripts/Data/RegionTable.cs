using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Region Table")]
    public sealed class RegionTable : AbstractTable<RegionModel, ERegion>
    {
        public RegionModel Get(ERegion key)
        {
            return Get((int)key);
        }
    }
}
