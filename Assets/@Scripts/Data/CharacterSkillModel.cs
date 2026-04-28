using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character Skill")]
    public sealed class CharacterSkillModel : AbstractModel<ECharacterSkill>
    {
        public SkillType SkillType;
        public Sprite Icon;

        [TextArea]
        public string Description;

        public int CostHeads;
        public int CostTails;
        public SkillEffect[] Effects;
        public EffectSO[] SpecialEffects;
        public int MaxPerTurn;
        public int MaxPerCombat;
    }
}
