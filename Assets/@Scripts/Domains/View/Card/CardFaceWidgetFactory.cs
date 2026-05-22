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
            PortraitCardFaceWidget widget = (PortraitCardFaceWidget)PortraitCardFaceWidget.Create();
            widget.Bind(viewModel);
            return widget;
        }

        private static ICardFaceWidget CreateChoice(ChoiceCardFaceViewModel viewModel)
        {
            ChoiceCardFaceWidget widget = (ChoiceCardFaceWidget)ChoiceCardFaceWidget.Create();
            widget.Bind(viewModel);
            return widget;
        }

        private static ICardFaceWidget CreateLocked(LockedCardFaceViewModel viewModel)
        {
            LockedCardFaceWidget widget = (LockedCardFaceWidget)LockedCardFaceWidget.Create();
            widget.Bind(viewModel);
            return widget;
        }
    }
}
