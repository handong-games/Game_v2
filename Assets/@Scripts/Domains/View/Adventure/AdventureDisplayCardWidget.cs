using Domains.Adventure;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Displays a non-combat Adventure board card without health, intent, or gameplay avatar bindings.
    [UxmlElement]
    public sealed partial class AdventureDisplayCardWidget : VisualElement
    {
        private const string WidgetName = "adventure-display-card";
        private const string CardWidgetName = "adventure-display-card-widget";

        private CardWidget _cardWidget;

        public AdventureDisplayCardWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public static AdventureDisplayCardWidget Create(VisualTreeAsset template)
        {
            if (template == null)
                throw new System.ArgumentNullException(nameof(template));

            TemplateContainer container = template.Instantiate();
            AdventureDisplayCardWidget widget = container.Q<AdventureDisplayCardWidget>(WidgetName);
            if (widget == null)
                throw new System.InvalidOperationException($"Required element missing: {WidgetName}");

            widget.RemoveFromHierarchy();
            widget.InitializeReferences();
            return widget;
        }

        public VisualElement InteractionTarget
        {
            get
            {
                EnsureReferences();
                return _cardWidget;
            }
        }

        public void Bind(
            CardViewModel card,
            AdventureCardWidgetTemplates templates)
        {
            if (templates == null)
                throw new System.ArgumentNullException(nameof(templates));

            EnsureReferences();
            _cardWidget.Bind(card, templates.FaceTemplates);
        }

        public void Unbind()
        {
            _cardWidget?.Unbind();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            EnsureReferences();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Unbind();
        }

        private void InitializeReferences()
        {
            _cardWidget ??= this.Q<CardWidget>(CardWidgetName);
        }

        private void EnsureReferences()
        {
            InitializeReferences();

            if (_cardWidget == null)
                throw new System.InvalidOperationException($"Required element missing: {CardWidgetName}");
        }
    }
}
