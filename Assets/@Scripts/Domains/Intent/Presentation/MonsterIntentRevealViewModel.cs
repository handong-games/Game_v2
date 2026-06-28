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
            CardId = cardId;
            Items = items ?? Array.Empty<IntentItemViewModel>();
        }

        public uint CardId { get; }
        public IReadOnlyList<IntentItemViewModel> Items { get; }
    }
}
