using Game.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class PortraitCardFaceWidget : VisualElement, ICardFaceWidget
    {
        private const string PortraitName = "card-portrait";
        private const string NameLabelName = "card-front-name";
        private const string Address = "PortraitCardFaceWidget";
        private const string WidgetName = "portrait-card";

        private static VisualTreeAsset _template;
        private VisualElement _portrait;
        private Label _name;
        private LocalizedString _localizedName;
        private PortraitCardFaceViewModel _pendingViewModel;

        public PortraitCardFaceWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static VisualElement Create()
        {
            TemplateContainer container = LoadTemplate().CloneTree();
            PortraitCardFaceWidget widget = container.Q<PortraitCardFaceWidget>(WidgetName);
            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(CardFaceViewModel viewModel)
        {
            Bind((PortraitCardFaceViewModel)viewModel);
        }

        public void Bind(PortraitCardFaceViewModel viewModel)
        {
            _pendingViewModel = viewModel;
            ApplyBinding();
        }

        public void Unbind()
        {
            if (_localizedName != null)
            {
                _localizedName.StringChanged -= SetName;
                _localizedName = null;
            }
        }

        private void SetName(string value)
        {
            _name.text = value ?? string.Empty;
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _portrait = this.Q<VisualElement>(PortraitName);
            _name = this.Q<Label>(NameLabelName);
        }

        private void ApplyBinding()
        {
            if (_portrait == null || _name == null || _pendingViewModel == null)
                return;

            Unbind();

            _portrait.style.backgroundImage = _pendingViewModel.Portrait != null
                ? new StyleBackground(Background.FromSprite(_pendingViewModel.Portrait))
                : StyleKeyword.Null;

            if (_pendingViewModel.LocalizedName == null || _pendingViewModel.LocalizedName.IsEmpty)
            {
                SetName(string.Empty);
                return;
            }

            _localizedName = _pendingViewModel.LocalizedName;
            SetName(_localizedName.GetLocalizedString());
            _localizedName.StringChanged += SetName;
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Unbind();
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
