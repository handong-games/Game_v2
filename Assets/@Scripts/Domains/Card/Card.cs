using Game.Data;
using Gameplay.GAS;

namespace Domains.Card
{
    public sealed class Card : GameplayActor
    {
        public Card(uint cardId, ICardModel model, ECardFace face)
        {
            CardId = cardId;
            Model = model;
            Face = face;
        }

        public uint CardId { get; }
        public ICardModel Model { get; private set; }
        public ECardFace Face { get; private set; }

        internal void SetModel(ICardModel model)
        {
            Model = model;
        }

        internal void SetFace(ECardFace face)
        {
            Face = face;
        }

        public void Flip()
        {
            Face = Face == ECardFace.Front
                ? ECardFace.Back
                : ECardFace.Front;
        }
    }
}
