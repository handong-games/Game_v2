using System;
using Domains.Character;
using Game.Data;
using Game.Generated;

namespace Domains.Run
{
    public sealed class RunState
    {
        public RunState(CharacterState character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            SelectedCharacterKey = character.Key;
            CharacterName = character.Name;
            CurrentHp = character.InitialMaxHp;
            MaxHp = character.InitialMaxHp;
            CoinCount = character.InitialCoinCount;
            DefaultSkills = character.DefaultSkills ?? Array.Empty<CharacterSkillModel>();
        }

        public ECharacter SelectedCharacterKey { get; }
        public string CharacterName { get; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; }
        public int CoinCount { get; private set; }
        public CharacterSkillModel[] DefaultSkills { get; }
    }
}
