using UnityEngine;
using Domains.Event;
using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private const string BannerHiddenClass = "banner--hidden";
        private const string BannerEnterClass = "banner--enter";
        private const string BannerExitClass = "banner--exit";
        private const string BannerRegionClass = "banner--region";
        private const string BannerTurnClass = "banner--turn";

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

        private const string CoinPouchHiddenClass = "card-board__coin-pouch--hidden";
        private const string CoinPouchEnterClass = "card-board__coin-pouch--enter";

        private async Awaitable PlayIntroAnimation()
        {
            PrepareBannerIntro();
            PrepareResourceStatusBarIntro();
            PrepareProgressBarIntro();
            PrepareCardDeckIntro();

            ViewTransitionTimeline timeline = new ViewTransitionTimeline()
                .Play(0, _adventureBanner, BannerEnterClass)
                .Play(1200, _adventureBanner, BannerExitClass)
                .Play(1700, _resourceStatusBar, ResourceStatusBarEnterClass)
                .Play(2050, _progressBar, ProgressBarEnterClass)
                .Play(2300, _progressBarTrackFill, ProgressBarTrackFillEnterClass)
                .Play(2600, _cardDeck, CardDeckEnterClass);

            await ViewTransitionManager.Instance.Play(timeline);

            AdventureEvents.IntroCompleted?.Invoke();
        }

        private async Awaitable PlayTurnBannerAnimation()
        {
            PrepareTurnBanner();
            PrepareCoinPouchHidden();

            ViewTransitionTimeline timeline = new ViewTransitionTimeline()
                .Play(0, _adventureBanner, BannerEnterClass)
                .Play(120, _coinPouch, CoinPouchEnterClass);

            await ViewTransitionManager.Instance.Play(timeline);
        }

        private void PrepareBannerIntro()
        {
            if (_adventureBanner == null)
                return;

            _adventureBanner.RemoveFromClassList(BannerTurnClass);
            _adventureBanner.AddToClassList(BannerRegionClass);
            _adventureBanner.RemoveFromClassList(BannerEnterClass);
            _adventureBanner.RemoveFromClassList(BannerExitClass);
            _adventureBanner.AddToClassList(BannerHiddenClass);
        }

        private void PrepareTurnBanner()
        {
            if (_adventureBanner == null)
                return;

            _adventureBanner.RemoveFromClassList(BannerRegionClass);
            _adventureBanner.AddToClassList(BannerTurnClass);
            _adventureBanner.RemoveFromClassList(BannerEnterClass);
            _adventureBanner.RemoveFromClassList(BannerExitClass);
            _adventureBanner.AddToClassList(BannerHiddenClass);
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

        private void PrepareCoinPouchHidden()
        {
            if (_coinPouch == null)
                return;

            _coinPouch.RemoveFromClassList(CoinPouchEnterClass);
            _coinPouch.AddToClassList(CoinPouchHiddenClass);
        }
    }
}
