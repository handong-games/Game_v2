using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character Table")]
    public sealed class CharacterTable : AbstractTable<CharacterModel, ECharacter>
    {
        public CharacterModel Get(ECharacter key)
        {
            return Get((int)key);
        }
    }
}
