using Domains.Card;
using Game.Data;

namespace Domains.Adventure
{
    using Card = global::Domains.Card.Card;

    public static class CardViewModelFactory
    {
        public static CardViewModel Create(Card card)
        {
            return Create(card.Model, card.Face);
        }

        public static CardViewModel Create(CardModelBase model, ECardFace face)
        {
            return new CardViewModel(
                face,
                CardFaceViewModelFactory.Create(model.Front),
                CardFaceViewModelFactory.Create(model.Back));
        }

        public static ECardFace GetDefaultFace(CardModelBase model)
        {
            if (model.HasOwnedTagExact(CardGameplayTags.KindChoice))
                return ECardFace.Back;

            return model.Front != null
                ? ECardFace.Front
                : ECardFace.Back;
        }
    }
}
