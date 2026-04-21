using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Skill Database")]
    public class SO_SkillDatabase : ScriptableObject
    {
        [SerializeField]
        private List<SO_SkillData> _skills = new List<SO_SkillData>();

        private Dictionary<int, SO_SkillData> _skillMapById;

        private void OnEnable()
        {
            BuildMap();
        }

        public void BuildMap()
        {
            _skillMapById = new Dictionary<int, SO_SkillData>();
            if (_skills == null)
                return;

            for (int i = 0; i < _skills.Count; i++)
            {
                SO_SkillData skill = _skills[i];
                if (skill == null || skill.Id <= 0)
                    continue;

                if (_skillMapById.ContainsKey(skill.Id))
                {
                    Debug.LogWarning($"[SO_SkillDatabase] Duplicate skill id: {skill.Id} ({skill.name})");
                    continue;
                }

                _skillMapById.Add(skill.Id, skill);
            }
        }

        public SO_SkillData GetSkill(int id)
        {
            if (_skillMapById == null)
            {
                BuildMap();
            }

            _skillMapById.TryGetValue(id, out SO_SkillData result);
            return result;
        }

        public List<SO_SkillData> GetAllSkills()
        {
            return _skills;
        }

        public void SetSkills(List<SO_SkillData> skills)
        {
            _skills = skills ?? new List<SO_SkillData>();
            BuildMap();
        }
    }
}
