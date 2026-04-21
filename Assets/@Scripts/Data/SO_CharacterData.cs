using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character")]
    public class SO_CharacterData : SO_IdentifiedData
    {
        public string CharacterId;
        public string CharacterName;
        public Sprite Illustration;
        public bool IsLocked;
        public int InitialCoinCount;
        public int InitialMaxHp;
        public SO_SkillData[] DefaultSkills;
    }
}
