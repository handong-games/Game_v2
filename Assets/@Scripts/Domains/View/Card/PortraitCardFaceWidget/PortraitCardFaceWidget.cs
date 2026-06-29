using Game.Data;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class PortraitCardFaceWidget : VisualElement, ICardFaceWidget
    {
        private const string PortraitName = "card-portrait";
        private const string NameLabelName = "card-front-name";
        private const string WidgetName = "portrait-card";

        private VisualElement _portrait;
        private Label _name;
        private LocalizedString _localizedName;
        private PortraitCardFaceViewModel _pendingViewModel;

        public PortraitCardFaceWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static VisualElement Create(VisualTreeAsset template)
        {
            if (template == null)
                throw new System.ArgumentNullException(nameof(template));

            TemplateContainer container = template.CloneTree();
            return Create(container);
        }

        private static VisualElement Create(TemplateContainer container)
        {
            PortraitCardFaceWidget widget = container.Q<PortraitCardFaceWidget>(WidgetName);
            if (widget == null)
                throw new System.InvalidOperationException($"Required element missing: {WidgetName}");

            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(CardFaceViewModel viewModel)
        {
            Bind((PortraitCardFaceViewModel)viewModel);
        }

        public void Bind(PortraitCardFaceViewModel viewModel)
        {
            if (viewModel == null)
                throw new System.ArgumentNullException(nameof(viewModel));

            _pendingViewModel = viewModel;
            InitializeReferences();
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
            InitializeReferences();
            EnsureReferences();
            ApplyBinding();
        }

        private void ApplyBinding()
        {
            if (_pendingViewModel == null)
                return;

            EnsureReferences();
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

        private void EnsureReferences()
        {
            if (_portrait == null)
                throw new System.InvalidOperationException($"Required element missing: {PortraitName}");

            if (_name == null)
                throw new System.InvalidOperationException($"Required element missing: {NameLabelName}");
        }

        private void InitializeReferences()
        {
            _portrait ??= this.Q<VisualElement>(PortraitName);
            _name ??= this.Q<Label>(NameLabelName);
        }

    }
}
