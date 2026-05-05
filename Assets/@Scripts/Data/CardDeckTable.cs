using Game.Generated;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Card Deck Table")]
    public sealed class CardDeckTable : AbstractTable<CardDeckModel, ECardDeck>
    {
        public CardDeckModel Get(ECardDeck key)
        {
            return Get((int)key);
        }
    }
}
