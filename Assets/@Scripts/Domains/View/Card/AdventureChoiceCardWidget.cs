using System;
using Domains.Adventure;
using Game.Data;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Renders one adventure choice card and owns only choice-card UI binding state.
    public sealed class AdventureChoiceCardWidget : BaseCardWidget
    {
        private const string IconName = "choice-card-icon";
        private const string LabelName = "choice-card-label";

        private readonly VisualElement _icon;
        private readonly Label _label;

        private LocalizedString _localizedLabel;
        private string _uiClassName;

        public AdventureChoiceCardWidget(VisualTreeAsset template)
            : base(template)
        {
            _icon = Content.Q<VisualElement>(IconName);
            if (_icon == null)
                throw new InvalidOperationException($"Required element missing: {IconName}");

            _label = Content.Q<Label>(LabelName);
            if (_label == null)
                throw new InvalidOperationException($"Required element missing: {LabelName}");

            RegisterCallback<DetachFromPanelEvent>(_ => Unbind());
        }

        public EChoiceCardType ChoiceType { get; private set; }

        public void Bind(
            AdventureChoiceCardViewModel viewModel,
            AdventureChoiceCardUIModel uiModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (uiModel == null)
                throw new ArgumentNullException(nameof(uiModel));

            Unbind();

            ChoiceType = viewModel.ChoiceType;

            string uiClassName = uiModel.UssClassName;
            if (string.IsNullOrWhiteSpace(uiClassName))
                throw new InvalidOperationException($"Choice card USS class is missing: {ChoiceType}");

            if (uiModel.Icon == null)
                throw new InvalidOperationException($"Choice card icon is missing: {ChoiceType}");

            _uiClassName = uiClassName;
            Root.AddToClassList(_uiClassName);
            _icon.style.backgroundImage = new StyleBackground(Background.FromSprite(uiModel.Icon));

            _localizedLabel = uiModel.DisplayName;
            if (_localizedLabel == null || _localizedLabel.IsEmpty)
                throw new InvalidOperationException($"Choice card display name is missing: {ChoiceType}");

            _label.text = _localizedLabel.GetLocalizedString();
            _localizedLabel.StringChanged += SetLabel;
        }

        public void Unbind()
        {
            if (_localizedLabel != null)
            {
                _localizedLabel.StringChanged -= SetLabel;
                _localizedLabel = null;
            }

            if (!string.IsNullOrWhiteSpace(_uiClassName))
            {
                Root.RemoveFromClassList(_uiClassName);
                _uiClassName = null;
            }
        }

        private void SetLabel(string value)
        {
            _label.text = value ?? string.Empty;
        }
    }
}
