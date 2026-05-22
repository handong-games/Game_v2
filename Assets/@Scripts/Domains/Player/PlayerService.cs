using System;
using System.Collections.Generic;
using Game.Core.Managers.Dependency;
using Game.Core.Utility;
using Game.Data;
using UnityEngine;
using UnityRandom = Unity.Mathematics.Random;

namespace Domains.Player
{
    using Card = global::Domains.Card.Card;

    [Dependency]
    public sealed class PlayerService : IDisposable
    {
        private UnityRandom _coinRandom;
        private Card _currentPlayerCard;

        public PlayerState CurrentPlayer { get; private set; }

        public void Initialize(CharacterModel character, uint seed)
        {
            CurrentPlayer = CreatePlayerState(character);
            _currentPlayerCard = null;
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
            _currentPlayerCard = null;
        }

        public void SetPlayerCard(Card card)
        {
            _currentPlayerCard = card;
        }

        public Card GetPlayerCard()
        {
            return _currentPlayerCard;
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
