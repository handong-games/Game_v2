namespace Domains.Adventure
{
    // Role:
    // Carries display data for a normal card placed on the Adventure board.
    // It does not carry runtime objects such as AbilitySystem or CardActor.
    public class AdventureBoardCardViewModel
    {
        public AdventureBoardCardViewModel(
            AdventureBoardSide side,
            uint cardId,
            CardViewModel card)
        {
            Side = side;
            CardId = cardId;
            Card = card;
        }

        public AdventureBoardSide Side { get; }
        public uint CardId { get; }
        public CardViewModel Card { get; }
    }
}
