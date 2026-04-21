using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Skill")]
    public class SO_SkillData : SO_IdentifiedData
    {
        public string SkillId;
        public string SkillName;

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
