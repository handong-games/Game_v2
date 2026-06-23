using System.Collections.Generic;
using Domains.View.Widgets;

namespace Domains.Adventure
{
    public sealed class AdventureInitialViewModel
    {
        public AdventureInitialViewModel(
            IReadOnlyList<AdventureSkillSlotViewModel> skillSlots,
            IReadOnlyList<AdventureCardViewModel> boardCards)
        {
            SkillSlots = skillSlots;
            BoardCards = boardCards;
        }

        public IReadOnlyList<AdventureSkillSlotViewModel> SkillSlots { get; }
        public IReadOnlyList<AdventureCardViewModel> BoardCards { get; }
    }
}
 
