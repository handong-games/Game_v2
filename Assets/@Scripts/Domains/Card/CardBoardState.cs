using System.Collections.Generic;

namespace Domains.Card
{
    public sealed class CardBoardState
    {
        private readonly Dictionary<ECardZone, List<uint>> _cardIdsByZone = new()
        {
            { ECardZone.Left, new List<uint>() },
            { ECardZone.Right, new List<uint>() },
            { ECardZone.Removed, new List<uint>() },
        };

        public List<uint> GetMutableCardIds(ECardZone zone)
        {
            return _cardIdsByZone[zone];
        }

        public IReadOnlyList<uint> GetCardIds(ECardZone zone)
        {
            return _cardIdsByZone[zone];
        }

        public IEnumerable<List<uint>> GetAllZones()
        {
            return _cardIdsByZone.Values;
        }
    }
}
