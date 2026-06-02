using Game.Data;
using Gameplay.GAS;
using UIToolkit.Timeline;

namespace Domains.Card
{
    public sealed class Card : GameplayActor
    {
        public Card(uint cardId, CardModelBase model, ECardFace face)
        {
            CardId = cardId;
            Model = model;
            Face = face;
        }

        public uint CardId { get; }
        public CardModelBase Model { get; private set; }
        public ECardFace Face { get; private set; }
        public UIToolkitTimelineComponent Timeline { get; } = new();

        internal void SetModel(CardModelBase model)
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
