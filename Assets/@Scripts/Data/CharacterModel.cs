using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Character")]
    public sealed class CharacterModel : AbstractModel<ECharacter>
    {
        public LocalizedString LocalizedName;
        public Sprite Portrait;
        public int InitialCoinCount;
        public int InitialMaxHp;
        public CharacterSkillModel[] DefaultSkills;
    }
}
