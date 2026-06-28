using UnityEngine;
using UnityEngine.Localization;

namespace Domains.Adventure
{
    // Role:
    // Carries localized and sprite presentation data for the first Adventure screen.
    // It does not load assets or decide animation timing.
    public sealed class AdventureEntryPresentationViewModel
    {
        public AdventureEntryPresentationViewModel(
            LocalizedString title,
            LocalizedString subtitle,
            Sprite background,
            Sprite emblem)
        {
            Title = title;
            Subtitle = subtitle;
            Background = background;
            Emblem = emblem;
        }

        public LocalizedString Title { get; }
        public LocalizedString Subtitle { get; }
        public Sprite Background { get; }
        public Sprite Emblem { get; }
    }

}
