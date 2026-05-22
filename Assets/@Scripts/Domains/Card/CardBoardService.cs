using System;
using System.Collections.Generic;
using Game.Core.Managers.Dependency;

namespace Domains.Card
{
    [Dependency]
    public sealed class CardBoardService : IDisposable
    {
        private readonly Dictionary<ECardZone, List<uint>> _cardIdsByZone = new()
        {
            { ECardZone.Left, new List<uint>() },
            { ECardZone.Right, new List<uint>() },
            { ECardZone.Removed, new List<uint>() },
        };

        public void PlaceCard(ECardZone zone, uint cardId)
        {
            RemoveCardFromAllZones(cardId);
            _cardIdsByZone[zone].Add(cardId);
        }

        public void PlaceCards(ECardZone zone, IReadOnlyList<uint> cardIds)
        {
            if (cardIds == null)
                return;

            for (int i = 0; i < cardIds.Count; i++)
            {
                PlaceCard(zone, cardIds[i]);
            }
        }

        public void RemoveCard(ECardZone zone, uint cardId)
        {
            _cardIdsByZone[zone].Remove(cardId);
        }

        public void MoveAllExcept(ECardZone zone, uint keepCardId, ECardZone targetZone)
        {
            List<uint> source = _cardIdsByZone[zone];
            List<uint> target = _cardIdsByZone[targetZone];

            for (int i = source.Count - 1; i >= 0; i--)
            {
                uint cardId = source[i];
                if (cardId == keepCardId)
                    continue;

                source.RemoveAt(i);
                target.Add(cardId);
            }
        }

        public IReadOnlyList<uint> GetCardIds(ECardZone zone)
        {
            return _cardIdsByZone[zone];
        }

        public void Clear()
        {
            foreach (List<uint> cardIds in _cardIdsByZone.Values)
            {
                cardIds.Clear();
            }
        }

        public void Dispose()
        {
            Clear();
        }

        private void RemoveCardFromAllZones(uint cardId)
        {
            foreach (List<uint> cardIds in _cardIdsByZone.Values)
            {
                cardIds.Remove(cardId);
            }
        }
    }
}
