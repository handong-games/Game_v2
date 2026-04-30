using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView : BaseView
    {
        private const int PreviewCardCount = 3;

        [Inject]
        private AdventureController _controller;

        private VisualElement _playerCardSlotGroup;
        private VisualElement _encounterCardSlotGroup;
        private VisualElement _adventureBanner;
        private VisualElement _resourceStatusBar;
        private VisualElement _resourceStatusBarRegion;
        private VisualElement _resourceStatusBarResources;
        private VisualElement _progressBar;
        private VisualElement _progressBarTrackFill;
        private VisualElement _cardDeck;

        protected override void OnBind(VisualElement root)
        {
            _playerCardSlotGroup = Root.Q<VisualElement>("adventure-player-card-slot-group");
            _encounterCardSlotGroup = Root.Q<VisualElement>("adventure-encounter-card-slot-group");
            _adventureBanner = Root.Q<VisualElement>("banner");
            _resourceStatusBar = Root.Q<VisualElement>("resource-status-bar");
            _resourceStatusBarRegion = Root.Q<VisualElement>("resource-status-bar-region");
            _resourceStatusBarResources = Root.Q<VisualElement>("resource-status-bar-resources");
            _progressBar = Root.Q<VisualElement>("progress-bar");
            _progressBarTrackFill = Root.Q<VisualElement>("progress-bar-track-fill");
            _cardDeck = Root.Q<VisualElement>("card-deck");

            RebuildCardSlots(_playerCardSlotGroup, "adventure-player-card", PreviewCardCount);
            RebuildCardSlots(_encounterCardSlotGroup, "adventure-encounter-card", PreviewCardCount);
        }
        
        protected override void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _ = PlayIntroAnimation();
        }

        private void RebuildCardSlots(VisualElement slotGroup, string cardNamePrefix, int count)
        {
            if (slotGroup == null)
            {
                return;
            }

            slotGroup.Clear();

            int cardCount = count < PreviewCardCount ? count : PreviewCardCount;
            for (int i = 0; i < cardCount; i++)
            {
                VisualElement slot = new VisualElement
                {
                    name = $"{cardNamePrefix}-slot-{i}"
                };
                slot.AddToClassList("adventure-view__card-slot");

                VisualElement card = CreateCard($"{cardNamePrefix}-{i}");
                slot.Add(card);
                slotGroup.Add(slot);
            }
        }

        private VisualElement CreateCard(string cardName)
        {
            VisualElement card = new VisualElement
            {
                name = cardName
            };
            card.AddToClassList("card");
            card.AddToClassList("adventure-view__card");

            VisualElement body = new VisualElement
            {
                name = "card-body"
            };
            body.AddToClassList("card__body");

            VisualElement frame = new VisualElement
            {
                name = "card-frame"
            };
            frame.AddToClassList("card__frame");

            VisualElement portrait = new VisualElement
            {
                name = "card-portrait"
            };
            portrait.AddToClassList("card__portrait");

            VisualElement overlay = new VisualElement
            {
                name = "card-overlay"
            };
            overlay.AddToClassList("card__overlay");

            VisualElement lockElement = new VisualElement
            {
                name = "card-lock"
            };
            lockElement.AddToClassList("card__lock");

            body.Add(frame);
            body.Add(portrait);
            body.Add(overlay);
            body.Add(lockElement);
            card.Add(body);

            return card;
        }
    }
}
