using System.Collections.Generic;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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

        [UxmlAttribute("turn-table")]
        public string TurnTable { get; set; }

        [UxmlAttribute("player-turn-key")]
        public string PlayerTurnKey { get; set; }

        [UxmlAttribute("enemy-turn-key")]
        public string EnemyTurnKey { get; set; }

        [UxmlAttribute("region-table")]
        public string RegionTable { get; set; }

        [UxmlAttribute("region-kicker-key")]
        public string RegionKickerKey { get; set; }

        [UxmlAttribute("region-name-key")]
        public string RegionNameKey { get; set; }

        public Banner()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public Awaitable PresentConfiguredRegion()
        {
            LocalizedString kicker = new(RegionTable, RegionKickerKey);
            LocalizedString regionName = new(RegionTable, RegionNameKey);

            return PresentRegion(
                GetLocalizedText(kicker),
                GetLocalizedText(regionName));
        }

        public Awaitable PresentPlayerTurn(int turnNumber)
        {
            LocalizedString turnText = new(TurnTable, PlayerTurnKey);
            string format = GetLocalizedText(turnText);

            string formattedText = LocalizationSettings.StringDatabase.SmartFormatter.Format(
                format,
                new Dictionary<string, object>
                {
                    ["turnNumber"] = turnNumber,
                });

            return PresentTurn(formattedText);
        }

        public Awaitable PresentEnemyTurn()
        {
            LocalizedString turnText = new(TurnTable, EnemyTurnKey);
            return PresentTurn(GetLocalizedText(turnText));
        }

        public async Awaitable PresentRegion(string kicker, string regionName)
        {
            SetMode(RegionClass, TurnClass);

            _regionKicker.text = kicker;
            _regionName.text = regionName;

            await Present();
        }

        public async Awaitable PresentTurn(string turnText)
        {
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

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _regionKicker = this.Q<Label>(RegionKickerName);
            _regionName = this.Q<Label>(RegionNameName);
            _turnText = this.Q<Label>(TurnTextName);
        }

        private static string GetLocalizedText(LocalizedString localizedString)
        {
            return localizedString == null || localizedString.IsEmpty
                ? string.Empty
                : localizedString.GetLocalizedString();
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
