using System;
using System.Collections.Generic;

namespace Domains.Intent.Presentation
{
    // Role:
    // UI-ready reveal data for one monster card's current intent.
    public sealed class MonsterIntentRevealViewModel
    {
        public MonsterIntentRevealViewModel(
            uint cardId,
            IReadOnlyList<IntentItemViewModel> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new InvalidOperationException("Monster intent reveal requires at least one item.");

            CardId = cardId;
            Items = items;
        }

        public uint CardId { get; }
        public IReadOnlyList<IntentItemViewModel> Items { get; }
    }
}
