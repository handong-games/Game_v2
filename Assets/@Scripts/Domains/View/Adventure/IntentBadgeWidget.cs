using Domains.Intent.Presentation;
using Game.Scenes.Adventure.Events.Widgets;
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
        private IntentBadgeWidgetEvents _events;

        public IntentBadgeWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public void Bind(IntentBadgeWidgetEvents events)
        {
            _events = events;
        }

        public async Awaitable Show(IntentItemViewModel item)
        {
            Apply(item);
            RemoveFromClassList(HiddenClass);
            RemoveFromClassList(RefreshClass);
            RemoveFromClassList(TriggeredClass);
            AddToClassList(RevealedClass);
            await Awaitable.NextFrameAsync();
            _events?.RevealCompleted?.Invoke();
        }

        public async Awaitable Refresh(IntentItemViewModel item)
        {
            Apply(item);
            RemoveFromClassList(RefreshClass);
            await Awaitable.NextFrameAsync();
            AddToClassList(RefreshClass);
            await Awaitable.NextFrameAsync();
            _events?.RefreshCompleted?.Invoke();
        }

        public async Awaitable Trigger()
        {
            RemoveFromClassList(RevealedClass);
            AddToClassList(TriggeredClass);
            await Awaitable.NextFrameAsync();
            _events?.TriggerCompleted?.Invoke();
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
            _icon = this.Q<VisualElement>(IconName);
            _number = this.Q<Label>(NumberName);
            Hide();
        }

        private void Apply(IntentItemViewModel item)
        {
            if (_icon == null || _number == null)
                return;

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
    }
}
