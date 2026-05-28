using System;
using Game.Core.Managers.Dependency;
using Game.Data;

namespace Domains.Player
{
    using Card = global::Domains.Card.Card;

    [Dependency]
    public sealed class PlayerService : IDisposable
    {
        private Card _currentPlayerCard;

        public PlayerState CurrentPlayer { get; private set; }

        public void Initialize(CharacterModel character, uint seed)
        {
            CurrentPlayer = new PlayerState(character);
            _currentPlayerCard = null;
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
    }
}
