using System;
using Gameplay.GAS;

namespace Domains.Combat
{
    using Card = global::Domains.Card.Card;

    public sealed class CombatCard
    {
        public CombatCard(Card card, ECombatSide side)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Side = side;
            Intent = new IntentComponent();
        }

        public uint CardId => Card.CardId;
        public Card Card { get; }
        public ECombatSide Side { get; }
        public AbilitySystemComponent AbilitySystem => Card.AbilitySystem;
        public IntentComponent Intent { get; }
    }
}
