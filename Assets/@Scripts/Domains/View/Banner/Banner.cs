using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class Banner : VisualElement
    {
        private const string RegionKickerName = "banner-region-kicker";
        private const string RegionNameName = "banner-region-name";
        private const string TurnTextName = "banner-turn-text";
        private const string HiddenClass = "banner--hidden";
        private const string EnterClass = "banner--enter";
        private const string ExitClass = "banner--exit";
        private const string RegionClass = "banner--region";
        private const string TurnClass = "banner--turn";
        private const float HoldSeconds = 0.5f;

        private Label _regionKicker;
        private Label _regionName;
        private Label _turnText;

        public async Awaitable PresentRegion(string kicker, string regionName)
        {
            EnsureInitialized();
            SetMode(RegionClass, TurnClass);

            _regionKicker.text = kicker;
            _regionName.text = regionName;

            await Present();
        }

        public async Awaitable PresentTurn(string turnText)
        {
            EnsureInitialized();
            SetMode(TurnClass, RegionClass);

            _turnText.text = turnText;

            await Present();
        }

        private async Awaitable Present()
        {
            await ViewTransitionManager.Instance.Play(this, EnterClass);
            await Awaitable.WaitForSecondsAsync(HoldSeconds);
            await ViewTransitionManager.Instance.Play(this, ExitClass);
        }

        private void EnsureInitialized()
        {
            _regionKicker ??= this.Q<Label>(RegionKickerName);
            _regionName ??= this.Q<Label>(RegionNameName);
            _turnText ??= this.Q<Label>(TurnTextName);
        }

        private void SetMode(string enabledClass, string disabledClass)
        {
            RemoveFromClassList(disabledClass);
            RemoveFromClassList(EnterClass);
            RemoveFromClassList(ExitClass);
            AddToClassList(enabledClass);
            AddToClassList(HiddenClass);
        }
    }
}
