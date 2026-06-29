using Domains.Adventure;
using Game.Data;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class LockedCardFaceWidget : VisualElement, ICardFaceWidget
    {
        private const string WidgetName = "locked-card";

        public static VisualElement Create(VisualTreeAsset template)
        {
            if (template == null)
                throw new System.ArgumentNullException(nameof(template));

            TemplateContainer container = template.CloneTree();
            return Create(container);
        }

        private static VisualElement Create(TemplateContainer container)
        {
            LockedCardFaceWidget widget = container.Q<LockedCardFaceWidget>(WidgetName);
            if (widget == null)
                throw new System.InvalidOperationException($"Required element missing: {WidgetName}");

            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(CardFaceViewModel viewModel)
        {
            Bind((LockedCardFaceViewModel)viewModel);
        }

        public void Bind(LockedCardFaceViewModel viewModel)
        {
        }

        public void Unbind()
        {
        }

    }
}
    
