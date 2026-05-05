using Game.Data;

namespace Domains.Adventure
{
    public sealed class CardState
    {
        public CardState(
            ICardModel model,
            ECardKind kind,
            ECardFace face,
            ECardBoardSide side)
        {
            Model = model;
            Kind = kind;
            Face = face;
            Side = side;
        }

        public ICardModel Model { get; }
        public ECardKind Kind { get; }
        public ECardFace Face { get; private set; }
        public ECardBoardSide Side { get; }

        public void Flip()
        {
            Face = Face == ECardFace.Front
                ? ECardFace.Back
                : ECardFace.Front;
        }
    }
}
