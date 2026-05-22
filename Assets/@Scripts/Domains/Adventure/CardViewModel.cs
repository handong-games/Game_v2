using Game.Data;

using Domains.Card;

namespace Domains.Adventure
{
    public sealed class CardViewModel
    {
        public CardViewModel(
            ECardFace face,
            CardFaceViewModel front,
            CardFaceViewModel back)
        {
            Face = face;
            Front = front;
            Back = back;
        }

        public ECardFace Face { get; }
        public CardFaceViewModel Front { get; }
        public CardFaceViewModel Back { get; }
    }
}
