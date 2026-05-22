using Domains.Adventure;
using Game.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class LockedCardFaceWidget : VisualElement, ICardFaceWidget
    {
        private const string Address = "LockedCardFaceWidget";
        private const string WidgetName = "locked-card";

        private static VisualTreeAsset _template;

        public static VisualElement Create()
        {
            TemplateContainer container = LoadTemplate().CloneTree();
            LockedCardFaceWidget widget = container.Q<LockedCardFaceWidget>(WidgetName);
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

        private static VisualTreeAsset LoadTemplate()
        {
            if (_template != null)
                return _template;

            _template = Addressables.LoadAssetAsync<VisualTreeAsset>(Address).WaitForCompletion();
            return _template;
        }
    }
}
    
