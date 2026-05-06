using System.Collections.Generic;
using Game.Data;

namespace Domains.Player
{
    public sealed class PlayerState
    {
        public PlayerState(
            CharacterModel character,
            int currentHp,
            int maxHp,
            int coinCount,
            List<CharacterSkillModel> skillSlots)
        {
            Character = character;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            CoinCount = coinCount;
            SkillSlots = skillSlots;
        }

        public CharacterModel Character { get; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public int CoinCount { get; private set; }
        public List<CharacterSkillModel> SkillSlots { get; }
    }
}
