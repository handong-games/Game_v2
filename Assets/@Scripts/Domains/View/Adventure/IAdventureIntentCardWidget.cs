using System.Collections.Generic;
using Domains.Intent.Presentation;
using UnityEngine;

namespace Domains.View.Widgets
{
    // Role:
    // Exposes intent badge presentation for board card widgets that support enemy intent.
    // Player and choice card widgets intentionally do not implement this.
    public interface IAdventureIntentCardWidget
    {
        Awaitable ShowIntentAsync(IReadOnlyList<IntentItemViewModel> items);
        Awaitable RefreshIntentAsync(IReadOnlyList<IntentItemViewModel> items);
        Awaitable TriggerIntentAsync();
    }
}
