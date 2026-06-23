using Game.Data;

namespace Domains.Player
{
    using Card = global::Domains.Card.Card;

    public sealed class PlayerRunState
    {
        public PlayerState CurrentPlayer { get; private set; }
        public Card CurrentPlayerCard { get; private set; }

        public void Initialize(CharacterModel character)
        {
            CurrentPlayer = new PlayerState(character);
            CurrentPlayerCard = null;
        }

        public void SetPlayerCard(Card card)
        {
            CurrentPlayerCard = card;
        }

        public void Clear()
        {
            CurrentPlayer = null;
            CurrentPlayerCard = null;
        }
    }
}
