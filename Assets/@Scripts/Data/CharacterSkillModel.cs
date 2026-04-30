using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character Skill")]
    public sealed class CharacterSkillModel : AbstractModel<ECharacterSkill>
    {
        [SerializeField]
        private SkillType _skillType;

        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        [TextArea]
        private string _description;

        [SerializeField]
        private int _costHeads;

        [SerializeField]
        private int _costTails;

        [SerializeField]
        private SkillEffect[] _effects;

        [SerializeField]
        private EffectSO[] _specialEffects;

        [SerializeField]
        private int _maxPerTurn;

        [SerializeField]
        private int _maxPerEncounter;

        public SkillType SkillType => _skillType;
        public Sprite Icon => _icon;
        public string Description => _description;
        public int CostHeads => _costHeads;
        public int CostTails => _costTails;
        public SkillEffect[] Effects => _effects;
        public EffectSO[] SpecialEffects => _specialEffects;
        public int MaxPerTurn => _maxPerTurn;
        public int MaxPerEncounter => _maxPerEncounter;
    }
}
