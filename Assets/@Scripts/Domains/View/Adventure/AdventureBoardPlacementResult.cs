using System;
using System.Collections.Generic;

namespace Domains.View.Widgets
{
    // Role:
    // Reports only the board sides changed by one PlaceCards call.
    public sealed class AdventureBoardPlacementResult
    {
        public AdventureBoardPlacementResult(
            IReadOnlyList<AdventureBoardCardPlacement> changedLeftPlacements,
            IReadOnlyList<AdventureBoardCardPlacement> changedRightPlacements)
        {
            ChangedLeftPlacements =
                changedLeftPlacements ?? Array.Empty<AdventureBoardCardPlacement>();
            ChangedRightPlacements =
                changedRightPlacements ?? Array.Empty<AdventureBoardCardPlacement>();
        }

        public IReadOnlyList<AdventureBoardCardPlacement> ChangedLeftPlacements { get; }
        public IReadOnlyList<AdventureBoardCardPlacement> ChangedRightPlacements { get; }

        public bool HasLeftChanges => ChangedLeftPlacements.Count > 0;
        public bool HasRightChanges => ChangedRightPlacements.Count > 0;
    }
}
