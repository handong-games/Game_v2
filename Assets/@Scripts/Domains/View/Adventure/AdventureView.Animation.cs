using UnityEngine;
using Domains.Event;
using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private const string ResourceStatusBarHiddenClass = "resource-status-bar--hidden";
        private const string ResourceStatusBarEnterClass = "resource-status-bar--enter";
        private const string ResourceStatusBarRegionHiddenClass = "resource-status-bar__region--hidden";
        private const string ResourceStatusBarRegionExitClass = "resource-status-bar__region--exit";
        private const string ResourceStatusBarResourcesHiddenClass = "resource-status-bar__resources--hidden";
        private const string ResourceStatusBarResourcesEnterClass = "resource-status-bar__resources--enter";

        private const string ProgressBarHiddenClass = "progress-bar--hidden";
        private const string ProgressBarEnterClass = "progress-bar--enter";
        private const string ProgressBarTrackFillHiddenClass = "progress-bar__track-fill--hidden";
        private const string ProgressBarTrackFillEnterClass = "progress-bar__track-fill--enter";

        private const string CardDeckHiddenClass = "card-deck--hidden";
        private const string CardDeckEnterClass = "card-deck--enter";

        private const int TurnBannerExitStartMs = 1200;

        private async Awaitable PlayIntroAnimation()
        {
            PrepareResourceStatusBarIntro();
            PrepareProgressBarIntro();
            PrepareCardDeckIntro();

            ViewTransitionTimeline timeline = new ViewTransitionTimeline()
                .Run(0, () => _banner.PresentRegion("지역", "전투 지역"))
                .Play(1700, _resourceStatusBar, ResourceStatusBarEnterClass)
                .Play(2050, _progressBar, ProgressBarEnterClass)
                .Play(2300, _progressBarTrackFill, ProgressBarTrackFillEnterClass)
                .Play(2600, _cardDeck, CardDeckEnterClass);

            await ViewTransitionManager.Instance.Play(timeline);

            AdventureEvents.IntroCompleted?.Invoke();
        }

        private async Awaitable PlayTurnBannerAnimation()
        {
            ViewTransitionTimeline timeline = new ViewTransitionTimeline();

            timeline
                .Run(0, () => _banner.PresentTurn("1 턴"))
                .Run(TurnBannerExitStartMs, _pouch.Show);

            await ViewTransitionManager.Instance.Play(timeline);
        }

        private void PrepareResourceStatusBarIntro()
        {
            if (_resourceStatusBar == null)
                return;

            _resourceStatusBar.RemoveFromClassList(ResourceStatusBarEnterClass);
            _resourceStatusBar.AddToClassList(ResourceStatusBarHiddenClass);
        }

        private void PrepareProgressBarIntro()
        {
            if (_progressBar == null ||
                _progressBarTrackFill == null)
            {
                return;
            }

            _progressBar.RemoveFromClassList(ProgressBarEnterClass);
            _progressBar.AddToClassList(ProgressBarHiddenClass);

            _progressBarTrackFill.RemoveFromClassList(ProgressBarTrackFillEnterClass);
            _progressBarTrackFill.AddToClassList(ProgressBarTrackFillHiddenClass);
        }

        private void PrepareCardDeckIntro()
        {
            if (_cardDeck == null)
            {
                return;
            }

            _cardDeck.RemoveFromClassList(CardDeckEnterClass);
            _cardDeck.AddToClassList(CardDeckHiddenClass);
        }
    }
}
