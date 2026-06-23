using System;
using System.Collections.Generic;

namespace Domains.Card
{
    public sealed class CardBoardService : IDisposable
    {
        private readonly CardBoardState _state;

        public CardBoardService(CardBoardState state)
        {
            _state = state;
        }

        public void PlaceCard(ECardZone zone, uint cardId)
        {
            RemoveCardFromAllZones(cardId);
            _state.GetMutableCardIds(zone).Add(cardId);
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
            _state.GetMutableCardIds(zone).Remove(cardId);
        }

        public void MoveAllExcept(ECardZone zone, uint keepCardId, ECardZone targetZone)
        {
            List<uint> source = _state.GetMutableCardIds(zone);
            List<uint> target = _state.GetMutableCardIds(targetZone);

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
            return _state.GetCardIds(zone);
        }

        public void Clear()
        {
            foreach (List<uint> cardIds in _state.GetAllZones())
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
            foreach (List<uint> cardIds in _state.GetAllZones())
            {
                cardIds.Remove(cardId);
            }
        }
    }
}
