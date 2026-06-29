using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class DamageNumberWidget : VisualElement
    {
        private const string RootClass = "damage-number-widget";
        private const string FloatingClass = "damage-number-widget--floating";
        private const string LabelClass = "damage-number-widget__label";
        private const string LabelName = "damage-number-widget-label";

        private readonly Label _label;

        public DamageNumberWidget()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(RootClass);

            _label = new Label
            {
                name = LabelName,
                pickingMode = PickingMode.Ignore,
            };
            _label.AddToClassList(LabelClass);
            Add(_label);
        }

        public void SetAmount(int amount)
        {
            _label.text = Mathf.Max(0, amount).ToString();
        }

        public void SetCenterPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }

        public async Awaitable Play()
        {
            RemoveFromClassList(FloatingClass);

            await Awaitable.NextFrameAsync();
            if (panel == null)
                return;

            Awaitable transition = ViewTransitionAwaiter.WaitForEnd(this);
            AddToClassList(FloatingClass);
            await transition;
        }

        public void Reset()
        {
            RemoveFromClassList(FloatingClass);
            style.left = 0f;
            style.top = 0f;
        }
    }
}
