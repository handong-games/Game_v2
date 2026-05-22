using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Monster Table")]
    public sealed class MonsterTable : AbstractTable<MonsterModel, EMonster>
    {
    }
}
