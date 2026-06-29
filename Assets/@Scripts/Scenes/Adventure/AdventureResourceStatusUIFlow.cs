using Domains.Adventure;
using Domains.View.Widgets;
using System;

namespace Game.Scenes.Adventure
{
    // Role:
    // Applies persistent Adventure HUD status data to ResourceStatusBar.
    // It does not play intro banner animation or mutate gameplay state.
    public sealed class AdventureResourceStatusUIFlow
    {
        public void Prepare(
            ResourceStatusBar resourceStatusBar,
            AdventureResourceStatusViewModel viewModel)
        {
            if (resourceStatusBar == null)
                throw new ArgumentNullException(nameof(resourceStatusBar));

            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            resourceStatusBar.SetRegion(viewModel.RegionName);
        }
    }
}
