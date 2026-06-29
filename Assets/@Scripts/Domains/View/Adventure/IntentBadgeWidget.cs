using Domains.Intent.Presentation;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class IntentBadgeWidget : VisualElement
    {
        private const string IconName = "intent-badge-icon";
        private const string NumberName = "intent-badge-number";
        private const string HiddenClass = "intent-badge--hidden";
        private const string RevealedClass = "intent-badge--revealed";
        private const string RefreshClass = "intent-badge--refresh";
        private const string TriggeredClass = "intent-badge--triggered";

        private VisualElement _icon;
        private Label _number;

        public IntentBadgeWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public async Awaitable Show(IntentItemViewModel item)
        {
            EnsureReferences();
            if (item == null)
                throw new System.ArgumentNullException(nameof(item));

            Apply(item);
            RemoveFromClassList(RevealedClass);
            RemoveFromClassList(RefreshClass);
            RemoveFromClassList(TriggeredClass);
            AddToClassList(HiddenClass);

            await Awaitable.NextFrameAsync();

            Awaitable transition = ViewTransitionAwaiter.WaitForEnd(this);
            RemoveFromClassList(HiddenClass);
            AddToClassList(RevealedClass);
            await transition;
        }

        public async Awaitable Refresh(IntentItemViewModel item)
        {
            EnsureReferences();
            if (item == null)
                throw new System.ArgumentNullException(nameof(item));

            Apply(item);
            RemoveFromClassList(RefreshClass);
            await Awaitable.NextFrameAsync();

            Awaitable pulseTransition = ViewTransitionAwaiter.WaitForEnd(this);
            AddToClassList(RefreshClass);
            await pulseTransition;

            Awaitable settleTransition = ViewTransitionAwaiter.WaitForEnd(this);
            RemoveFromClassList(RefreshClass);
            await settleTransition;
        }

        public async Awaitable Trigger()
        {
            RemoveFromClassList(HiddenClass);
            RemoveFromClassList(RefreshClass);
            RemoveFromClassList(TriggeredClass);
            AddToClassList(RevealedClass);

            await Awaitable.NextFrameAsync();

            Awaitable transition = ViewTransitionAwaiter.WaitForEnd(this);
            RemoveFromClassList(RevealedClass);
            AddToClassList(TriggeredClass);
            await transition;
        }

        public void Hide()
        {
            RemoveFromClassList(RevealedClass);
            RemoveFromClassList(RefreshClass);
            RemoveFromClassList(TriggeredClass);
            AddToClassList(HiddenClass);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            EnsureReferences();
            Hide();
        }

        private void Apply(IntentItemViewModel item)
        {
            EnsureReferences();

            if (item?.Icon != null)
            {
                _icon.style.backgroundImage = new StyleBackground(Background.FromSprite(item.Icon));
            }
            else
            {
                _icon.style.backgroundImage = StyleKeyword.Null;
            }

            _number.text = item?.NumberText ?? string.Empty;
            _number.style.display = item != null && item.HasNumber
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void InitializeReferences()
        {
            _icon ??= this.Q<VisualElement>(IconName);
            _number ??= this.Q<Label>(NumberName);
        }

        private void EnsureReferences()
        {
            InitializeReferences();

            if (_icon == null)
                throw new System.InvalidOperationException($"Required element missing: {IconName}");

            if (_number == null)
                throw new System.InvalidOperationException($"Required element missing: {NumberName}");
        }
    }
}
