using System;
using System.Collections.Generic;
using Game.Core.Managers.Dependency;
using Gameplay.GAS;

namespace Domains.Combat
{
    using Card = global::Domains.Card.Card;

    public sealed class CombatCard
    {
        public CombatCard(Card card, ECombatSide side)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Side = side;
        }

        public uint CardId => Card.CardId;
        public Card Card { get; }
        public ECombatSide Side { get; }
        public AbilitySystemComponent AbilitySystem => Card.AbilitySystem;
    }

    [Dependency]
    public sealed class CombatService : IDisposable
    {
        private readonly List<CombatCard>[] _cardsBySide =
            new List<CombatCard>[(int)ECombatSide.Count];

        public CombatService()
        {
            for (int i = 0; i < _cardsBySide.Length; i++)
            {
                _cardsBySide[i] = new List<CombatCard>();
            }
        }

        private ECombatSide _currentSide;

        public int RoundNumber { get; private set; }
        public ECombatSide CurrentSide => _currentSide;
        public IReadOnlyList<CombatCard> PlayerCards => _cardsBySide[(int)ECombatSide.Player];
        public IReadOnlyList<CombatCard> EnemyCards => _cardsBySide[(int)ECombatSide.Enemy];

        public void ReadyCombat(IReadOnlyList<CombatCard> combatCards)
        {
            if (combatCards == null)
                throw new ArgumentNullException(nameof(combatCards));

            ClearCards();

            for (int i = 0; i < combatCards.Count; i++)
            {
                CombatCard combatCard = combatCards[i];
                if (combatCard == null)
                    continue;

                _cardsBySide[(int)combatCard.Side].Add(combatCard);
            }

            int playerCount = _cardsBySide[(int)ECombatSide.Player].Count;
            if (playerCount != 1)
            {
                throw new InvalidOperationException(
                    $"ReadyCombat expects exactly one player card, but found {playerCount}.");
            }

            _currentSide = ECombatSide.Enemy;
            RoundNumber = 0;
        }

        public void NextTurn()
        {
            _currentSide = GetNextSide();

            if (_currentSide == ECombatSide.Player)
                RoundNumber++;
        }

        public IReadOnlyList<CombatCard> GetCards(ECombatSide side)
        {
            return _cardsBySide[(int)side];
        }

        public void Dispose()
        {
            ClearCards();
            _currentSide = ECombatSide.Enemy;
            RoundNumber = 0;
        }

        private ECombatSide GetNextSide()
        {
            return _currentSide == ECombatSide.Player
                ? ECombatSide.Enemy
                : ECombatSide.Player;
        }

        private void ClearCards()
        {
            for (int i = 0; i < _cardsBySide.Length; i++)
            {
                _cardsBySide[i].Clear();
            }
        }

    }
}
