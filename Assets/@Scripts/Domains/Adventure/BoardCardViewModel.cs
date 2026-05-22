namespace Domains.Adventure
{
    public sealed class BoardCardViewModel
    {
        public BoardCardViewModel(uint cardId, CardViewModel card, CardHealthBinding health = null)
        {
            CardId = cardId;
            Card = card;
            Health = health;
        }

        public uint CardId { get; }
        public CardViewModel Card { get; }
        public CardHealthBinding Health { get; }
        public bool HasHealth => Health != null && Health.IsValid;
    }
}
