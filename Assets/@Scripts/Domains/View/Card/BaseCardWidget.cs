using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Reads the common card shell contract from a concrete card UXML and provides shared card state helpers.
    public abstract class BaseCardWidget : VisualElement
    {
        private const string RootName = "card-root";
        private const string BackgroundName = "card-background";
        private const string FrameName = "card-frame";
        private const string ContentName = "card-content";
        private const string SelectedClass = "card-widget--selected";
        private const string DisabledClass = "card-widget--disabled";

        protected BaseCardWidget(VisualTreeAsset template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            TemplateContainer container = template.CloneTree();

            Root = container.Q<VisualElement>(RootName);
            if (Root == null)
                throw new InvalidOperationException($"Required element missing: {RootName}");

            Root.RemoveFromHierarchy();
            hierarchy.Add(Root);

            CardBackground = Root.Q<VisualElement>(BackgroundName);
            Frame = Root.Q<VisualElement>(FrameName);
            Content = Root.Q<VisualElement>(ContentName);

            if (CardBackground == null)
                throw new InvalidOperationException($"Required element missing: {BackgroundName}");

            if (Frame == null)
                throw new InvalidOperationException($"Required element missing: {FrameName}");

            if (Content == null)
                throw new InvalidOperationException($"Required element missing: {ContentName}");
        }

        protected VisualElement Root { get; }
        protected VisualElement CardBackground { get; }
        protected VisualElement Frame { get; }
        protected VisualElement Content { get; }

        public void SetSelected(bool selected)
        {
            Root.EnableInClassList(SelectedClass, selected);
        }

        public void SetDisabled(bool disabled)
        {
            Root.EnableInClassList(DisabledClass, disabled);
        }
    }
}
