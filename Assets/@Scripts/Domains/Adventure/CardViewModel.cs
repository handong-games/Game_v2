using System;
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
            if (face == ECardFace.Front && front == null)
                throw new ArgumentException("Front card face view model is required when the visible face is Front.", nameof(front));

            if (face == ECardFace.Back && back == null)
                throw new ArgumentException("Back card face view model is required when the visible face is Back.", nameof(back));

            Face = face;
            Front = front;
            Back = back;
        }

        public ECardFace Face { get; }
        public CardFaceViewModel Front { get; }
        public CardFaceViewModel Back { get; }
    }
}
