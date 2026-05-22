using Domains.Adventure;
using Game.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class ChoiceCardFaceWidget : VisualElement, ICardFaceWidget
    {
        private const string Address = "ChoiceCardFaceWidget";
        private const string WidgetName = "choice-card";
        private const string IconName = "choice-card-icon";
        private const string LabelName = "choice-card-label";
        private const string MonsterClass = "choice-card--monster";
        private const string EliteClass = "choice-card--elite";
        private const string BossClass = "choice-card--boss";
        private const string EventClass = "choice-card--event";
        private const string ShopClass = "choice-card--shop";

        private static VisualTreeAsset _template;
        private VisualElement _icon;
        private Label _label;
        private LocalizedString _localizedLabel;

        public ChoiceCardFaceWidget()
        {
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static VisualElement Create()
        {
            TemplateContainer container = LoadTemplate().CloneTree();
            ChoiceCardFaceWidget widget = container.Q<ChoiceCardFaceWidget>(WidgetName);
            widget.RemoveFromHierarchy();
            return widget;
        }

        public void Bind(CardFaceViewModel viewModel)
        {
            Bind((ChoiceCardFaceViewModel)viewModel);
        }

        public void Bind(ChoiceCardFaceViewModel viewModel)
        {
            _icon ??= this.Q<VisualElement>(IconName);
            _label ??= this.Q<Label>(LabelName);

            Unbind();
            ApplyStyleClass(viewModel.StyleType);

            _icon.style.backgroundImage = viewModel.Icon != null
                ? new StyleBackground(Background.FromSprite(viewModel.Icon))
                : StyleKeyword.Null;

            if (viewModel.Label == null || viewModel.Label.IsEmpty)
            {
                SetLabel(GetFallbackLabel(viewModel.StyleType));
                return;
            }

            _localizedLabel = viewModel.Label;
            SetLabel(_localizedLabel.GetLocalizedString());
            _localizedLabel.StringChanged += SetLabel;
        }

        public void Unbind()
        {
            if (_localizedLabel != null)
            {
                _localizedLabel.StringChanged -= SetLabel;
                _localizedLabel = null;
            }
        }

        private void SetLabel(string value)
        {
            _label.text = value ?? string.Empty;
        }

        private void ApplyStyleClass(EChoiceCardType styleType)
        {
            RemoveFromClassList(MonsterClass);
            RemoveFromClassList(EliteClass);
            RemoveFromClassList(BossClass);
            RemoveFromClassList(EventClass);
            RemoveFromClassList(ShopClass);

            AddToClassList(styleType switch
            {
                EChoiceCardType.Elite => EliteClass,
                EChoiceCardType.Boss => BossClass,
                EChoiceCardType.Event => EventClass,
                EChoiceCardType.Shop => ShopClass,
                _ => MonsterClass,
            });
        }

        private static string GetFallbackLabel(EChoiceCardType styleType)
        {
            return styleType switch
            {
                EChoiceCardType.Elite => "Elite",
                EChoiceCardType.Boss => "Boss",
                EChoiceCardType.Event => "Event",
                EChoiceCardType.Shop => "Shop",
                _ => "Monster",
            };
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
