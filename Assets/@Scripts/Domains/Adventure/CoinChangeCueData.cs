using System.Collections.Generic;
using Domains.Player;

namespace Domains.Adventure
{
    public readonly struct CoinChangeCueEntry
    {
        public CoinChangeCueEntry(ECoinFace face, int delta)
        {
            Face = face;
            Delta = delta;
        }

        public ECoinFace Face { get; }
        public int Delta { get; }
    }

    public sealed class CoinChangeCueData
    {
        public CoinChangeCueData(IReadOnlyList<CoinChangeCueEntry> entries)
        {
            Entries = entries;
        }

        public IReadOnlyList<CoinChangeCueEntry> Entries { get; }
        public bool HasEntries => Entries != null && Entries.Count > 0;
    }
}
