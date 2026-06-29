using Domains.Card;
using Game.Data;
using System;

namespace Domains.Adventure
{
    using Card = global::Domains.Card.Card;

    public static class CardViewModelFactory
    {
        public static CardViewModel Create(Card card)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));

            return Create(card.Model, card.Face);
        }

        public static CardViewModel Create(CardModelBase model, ECardFace face)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new CardViewModel(
                face,
                CardFaceViewModelFactory.CreateOptional(model.Front),
                CardFaceViewModelFactory.CreateOptional(model.Back));
        }

        public static ECardFace GetDefaultFace(CardModelBase model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (model.HasOwnedTagExact(CardGameplayTags.KindChoice))
                return ECardFace.Back;

            return model.Front != null
                ? ECardFace.Front
                : ECardFace.Back;
        }
    }
}
