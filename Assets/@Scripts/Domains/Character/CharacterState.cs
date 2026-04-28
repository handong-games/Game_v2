using Game.Data;
using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Domains.Character
{
    public sealed class CharacterState
    {
        public CharacterState(CharacterModel model)
        {
            Model = model;
            Key = model.Key;
            Name = model.Name;
            LocalizedName = model.LocalizedName;
            Portrait = model.Portrait;
            InitialCoinCount = model.InitialCoinCount;
            InitialMaxHp = model.InitialMaxHp;
            DefaultSkills = model.DefaultSkills;
        }

        public CharacterModel Model { get; }
        public ECharacter Key { get; }
        public string Name { get; }
        public LocalizedString LocalizedName { get; }
        public Sprite Portrait { get; }
        public int InitialCoinCount { get; }
        public int InitialMaxHp { get; }
        public CharacterSkillModel[] DefaultSkills { get; }
        public bool IsUnlocked { get; internal set; }
    }
}
