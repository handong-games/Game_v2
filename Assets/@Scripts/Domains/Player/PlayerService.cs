using System;
using Game.Data;

namespace Domains.Player
{
    using Card = global::Domains.Card.Card;

    public sealed class PlayerService : IDisposable
    {
        private readonly PlayerRunState _state;

        public PlayerService(PlayerRunState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public PlayerState CurrentPlayer => _state.CurrentPlayer;

        public void Initialize(CharacterModel character)
        {
            _state.Initialize(character);
        }

        public void Dispose()
        {
            _state.Clear();
        }

        public void SetPlayerCard(Card card)
        {
            _state.SetPlayerCard(card);
        }

        public Card GetPlayerCard()
        {
            return _state.CurrentPlayerCard;
        }
    }
}
