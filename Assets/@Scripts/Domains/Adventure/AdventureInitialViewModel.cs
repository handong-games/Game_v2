using System.Collections.Generic;
using Domains.View.Widgets;

namespace Domains.Adventure
{
    public sealed class AdventureInitialViewModel
    {
        public AdventureInitialViewModel(IReadOnlyList<SkillSlotViewModelV2> skillSlots)
        {
            SkillSlots = skillSlots;
        }

        public IReadOnlyList<SkillSlotViewModelV2> SkillSlots { get; }
    }
}
