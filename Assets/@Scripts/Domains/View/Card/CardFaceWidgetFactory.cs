using System;
using Game.Data;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    public static class CardFaceWidgetFactory
    {
        public static VisualElement Create(CardFaceViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            ICardFaceWidget widget = viewModel switch
            {
                PortraitCardFaceViewModel portrait => CreatePortrait(portrait),
                ChoiceCardFaceViewModel choice => CreateChoice(choice),
                LockedCardFaceViewModel locked => CreateLocked(locked),
                _ => throw new ArgumentOutOfRangeException(nameof(viewModel)),
            };

            return (VisualElement)widget;
        }

        private static ICardFaceWidget CreatePortrait(PortraitCardFaceViewModel viewModel)
        {
            return (PortraitCardFaceWidget)PortraitCardFaceWidget.Create();
        }

        private static ICardFaceWidget CreateChoice(ChoiceCardFaceViewModel viewModel)
        {
            return (ChoiceCardFaceWidget)ChoiceCardFaceWidget.Create();
        }

        private static ICardFaceWidget CreateLocked(LockedCardFaceViewModel viewModel)
        {
            return (LockedCardFaceWidget)LockedCardFaceWidget.Create();
        }
    }
}
