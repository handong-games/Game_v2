using System.Collections.Generic;
using System;
using Domains.View.Widgets;

namespace Domains.Adventure
{
    // Role:
    // Carries the first Adventure screen payload prepared by the controller.
    // It contains only the data needed to prepare and play the intro scope.
    public sealed class AdventureInitialPresentationViewModel
    {
        public AdventureInitialPresentationViewModel(
            AdventureEntryPresentationViewModel entry,
            AdventureResourceStatusViewModel resourceStatus,
            AdventureBoardPresentationViewModel board,
            IReadOnlyList<AdventureSkillSlotViewModel> skillSlots)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
            ResourceStatus = resourceStatus ?? throw new ArgumentNullException(nameof(resourceStatus));
            Board = board ?? throw new ArgumentNullException(nameof(board));
            SkillSlots = skillSlots ?? Array.Empty<AdventureSkillSlotViewModel>();
        }

        public AdventureEntryPresentationViewModel Entry { get; }
        public AdventureResourceStatusViewModel ResourceStatus { get; }
        public AdventureBoardPresentationViewModel Board { get; }
        public IReadOnlyList<AdventureBoardCardViewModel> BoardCards => Board.DisplayCards;
        public IReadOnlyList<AdventureCardViewModel> RuntimeBoardCards => Board.RuntimeCards;
        public IReadOnlyList<AdventureSkillSlotViewModel> SkillSlots { get; }
    }
}
