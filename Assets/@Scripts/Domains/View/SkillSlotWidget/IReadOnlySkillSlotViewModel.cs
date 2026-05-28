using UnityEngine;
using UnityEngine.Localization;

namespace Domains.View.Widgets
{
    public interface IReadOnlySkillSlotViewModel
    {
        LocalizedString Name { get; }
        Sprite Icon { get; }
    }
}
