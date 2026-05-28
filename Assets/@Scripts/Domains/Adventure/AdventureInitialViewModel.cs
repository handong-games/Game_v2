using System.Collections.Generic;
using Domains.View.Widgets;

namespace Domains.Adventure
{
    public sealed class AdventureInitialViewModel
    {
        public AdventureInitialViewModel(IReadOnlyList<AdventureSkillSlotViewModel> skillSlots)
        {
            SkillSlots = skillSlots;
        }

        public IReadOnlyList<AdventureSkillSlotViewModel> SkillSlots { get; }
    }
}
