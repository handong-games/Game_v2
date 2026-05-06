using Domains.Player;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class CoinStatusWidget : VisualElement
    {
        private const string HeadsLabelName = "coin-status-heads";
        private const string TailsLabelName = "coin-status-tails";
        private const string HeadsTargetName = "coin-status-heads-icon";
        private const string TailsTargetName = "coin-status-tails-icon";
        private const string HiddenClass = "ui-transition--hidden";
        private const string FromBottomClass = "ui-transition--from-bottom";
        private const string EnterClass = "ui-transition--enter";

        private Label _headsLabel;
        private Label _tailsLabel;
        private int _headsCount;
        private int _tailsCount;
        private bool _isShown;

        public VisualElement HeadsTarget { get; private set; }
        public VisualElement TailsTarget { get; private set; }

        public CoinStatusWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public void SetCount(int heads, int tails)
        {
            _headsCount = heads;
            _tailsCount = tails;
            Refresh();
        }

        public void Reset()
        {
            SetCount(0, 0);
        }

        public void Add(ECoinFace face)
        {
            if (face == ECoinFace.Heads)
            {
                SetCount(_headsCount + 1, _tailsCount);
                return;
            }

            SetCount(_headsCount, _tailsCount + 1);
        }

        public VisualElement GetTarget(ECoinFace face)
        {
            return face == ECoinFace.Heads
                ? HeadsTarget
                : TailsTarget;
        }

        public async Awaitable Show()
        {
            if (_isShown)
                return;

            _isShown = true;
            SetHidden();

            await ViewTransitionManager.Instance.Play(this, EnterClass);
        }

        public void Hide()
        {
            _isShown = false;
            SetHidden();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            Bind();
            Refresh();
        }

        private void Bind()
        {
            _headsLabel = this.Q<Label>(HeadsLabelName);
            _tailsLabel = this.Q<Label>(TailsLabelName);
            HeadsTarget = this.Q<VisualElement>(HeadsTargetName);
            TailsTarget = this.Q<VisualElement>(TailsTargetName);
        }

        private void Refresh()
        {
            if (_headsLabel != null)
                _headsLabel.text = _headsCount.ToString();

            if (_tailsLabel != null)
                _tailsLabel.text = _tailsCount.ToString();
        }

        private void SetHidden()
        {
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }
    }
}
