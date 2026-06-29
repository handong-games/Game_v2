using System;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Carries preloaded shared card face UXML templates.
    public sealed class CardFaceWidgetTemplates
    {
        public CardFaceWidgetTemplates(
            VisualTreeAsset portraitFace,
            VisualTreeAsset lockedFace)
        {
            PortraitFace = portraitFace ?? throw new ArgumentNullException(nameof(portraitFace));
            LockedFace = lockedFace ?? throw new ArgumentNullException(nameof(lockedFace));
        }

        public VisualTreeAsset PortraitFace { get; }
        public VisualTreeAsset LockedFace { get; }
    }
}
