using System;
using Game.Data;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    public static class CardFaceWidgetFactory
    {
        public static VisualElement Create(
            CardFaceViewModel viewModel,
            CardFaceWidgetTemplates templates)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (templates == null)
                throw new ArgumentNullException(nameof(templates));

            ICardFaceWidget widget = viewModel switch
            {
                PortraitCardFaceViewModel portrait => CreatePortrait(portrait, templates),
                LockedCardFaceViewModel locked => CreateLocked(locked, templates),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel)),
            };

            return (VisualElement)widget;
        }

        private static ICardFaceWidget CreatePortrait(
            PortraitCardFaceViewModel viewModel,
            CardFaceWidgetTemplates templates)
        {
            return (PortraitCardFaceWidget)PortraitCardFaceWidget.Create(templates.PortraitFace);
        }

        private static ICardFaceWidget CreateLocked(
            LockedCardFaceViewModel viewModel,
            CardFaceWidgetTemplates templates)
        {
            return (LockedCardFaceWidget)LockedCardFaceWidget.Create(templates.LockedFace);
        }
    }
}
