using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Card Table")]
    public sealed class CardTable : AbstractTable<BasicCardModel, ECard>
    {
    }
}
