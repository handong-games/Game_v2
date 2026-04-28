using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character Skill Table")]
    public sealed class CharacterSkillTable : AbstractTable<CharacterSkillModel, ECharacterSkill>
    {
        public CharacterSkillModel Get(ECharacterSkill key)
        {
            return Get((int)key);
        }
    }
}
