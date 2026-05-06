using System;
using System.Collections.Generic;
using Game.Core.Managers.Dependency;
using Game.Core.Utility;
using Game.Data;
using UnityEngine;
using UnityRandom = Unity.Mathematics.Random;

namespace Domains.Player
{
    [Dependency]
    public sealed class PlayerService : IDisposable
    {
        private UnityRandom _coinRandom;

        public PlayerState CurrentPlayer { get; private set; }

        public void Initialize(CharacterModel character, uint seed)
        {
            CurrentPlayer = CreatePlayerState(character);
            _coinRandom = new UnityRandom(RandomUtility.CombineSeed(seed, "Coin"));
        }

        public CoinFlipDto OpenPouch()
        {
            ECoinFace[] faces = new ECoinFace[CurrentPlayer.CoinCount];

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = _coinRandom.NextFloat() < 0.5f
                    ? ECoinFace.Heads
                    : ECoinFace.Tails;
            }

            return new CoinFlipDto(faces);
        }

        public void Dispose()
        {
            CurrentPlayer = null;
        }

        private static PlayerState CreatePlayerState(CharacterModel character)
        {
            int slotCount = Mathf.Clamp(character.DefaultSkillSlotCount, 1, character.MaxSkillSlotCount);
            IReadOnlyList<CharacterSkillModel> defaultSkills = character.DefaultSkills;
            List<CharacterSkillModel> skillSlots = new(slotCount);

            for (int i = 0; i < slotCount; i++)
            {
                CharacterSkillModel skill = defaultSkills != null && i < defaultSkills.Count
                    ? defaultSkills[i]
                    : null;

                skillSlots.Add(skill);
            }

            return new PlayerState(
                character,
                character.MaxHp,
                character.MaxHp,
                character.CoinCount,
                skillSlots);
        }
    }
}
