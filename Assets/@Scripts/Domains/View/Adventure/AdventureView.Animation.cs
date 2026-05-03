using UnityEngine;
using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private const string BannerHiddenClass = "banner--hidden";
        private const string BannerEnterClass = "banner--enter";
        private const string BannerExitClass = "banner--exit";
        private const float BannerHoldSeconds = 0.7f;

        private const string ResourceStatusBarHiddenClass = "resource-status-bar--hidden";
        private const string ResourceStatusBarEnterClass = "resource-status-bar--enter";
        private const string ResourceStatusBarRegionHiddenClass = "resource-status-bar__region--hidden";
        private const string ResourceStatusBarRegionExitClass = "resource-status-bar__region--exit";
        private const string ResourceStatusBarResourcesHiddenClass = "resource-status-bar__resources--hidden";
        private const string ResourceStatusBarResourcesEnterClass = "resource-status-bar__resources--enter";
        private const float ResourceStatusBarRegionHoldSeconds = 0.4f;

        private const string ProgressBarHiddenClass = "progress-bar--hidden";
        private const string ProgressBarEnterClass = "progress-bar--enter";
        private const string ProgressBarTrackFillHiddenClass = "progress-bar__track-fill--hidden";
        private const string ProgressBarTrackFillEnterClass = "progress-bar__track-fill--enter";

        private const string CardDeckHiddenClass = "card-deck--hidden";
        private const string CardDeckEnterClass = "card-deck--enter";

        private async Awaitable PlayIntroAnimation()
        {
            PrepareBannerIntro();
            PrepareResourceStatusBarIntro();
            PrepareProgressBarIntro();
            PrepareCardDeckIntro();

            await ViewTransitionManager.Instance.Play(
                _adventureBanner,
                BannerEnterClass);
            
            await Awaitable.WaitForSecondsAsync(BannerHoldSeconds);
            
            await ViewTransitionManager.Instance.Play(
                _adventureBanner,
                BannerExitClass);

            await PlayResourceStatusBarIntroAnimation();
            await PlayProgressBarIntroAnimation();
            await PlayCardDeckIntroAnimation();
            await PlayPlaceholderCardsAsync();
        }

        private void PrepareBannerIntro()
        {
            if (_adventureBanner == null)
                return;

            _adventureBanner.RemoveFromClassList(BannerEnterClass);
            _adventureBanner.RemoveFromClassList(BannerExitClass);
            _adventureBanner.AddToClassList(BannerHiddenClass);
        }

        private async Awaitable PlayResourceStatusBarIntroAnimation()
        {
            if (_resourceStatusBar == null ||
                _resourceStatusBarRegion == null ||
                _resourceStatusBarResources == null)
            {
                return;
            }

            await ViewTransitionManager.Instance.Play(
                _resourceStatusBar,
                ResourceStatusBarEnterClass);

            /*await Awaitable.WaitForSecondsAsync(ResourceStatusBarRegionHoldSeconds);

            await ViewTransitionManager.Instance.Play(
                _resourceStatusBarRegion,
                ResourceStatusBarRegionExitClass);

            await ViewTransitionManager.Instance.Play(
                _resourceStatusBarResources,
                ResourceStatusBarResourcesEnterClass);*/
        }

        private void PrepareResourceStatusBarIntro()
        {
            if (_resourceStatusBar == null ||
                _resourceStatusBarRegion == null ||
                _resourceStatusBarResources == null)
            {
                return;
            }

            _resourceStatusBar.RemoveFromClassList(ResourceStatusBarEnterClass);
            _resourceStatusBar.AddToClassList(ResourceStatusBarHiddenClass);

            _resourceStatusBarRegion.RemoveFromClassList(ResourceStatusBarRegionExitClass);
            _resourceStatusBarRegion.RemoveFromClassList(ResourceStatusBarRegionHiddenClass);

            _resourceStatusBarResources.RemoveFromClassList(ResourceStatusBarResourcesEnterClass);
            _resourceStatusBarResources.AddToClassList(ResourceStatusBarResourcesHiddenClass);
        }

        private async Awaitable PlayProgressBarIntroAnimation()
        {
            if (_progressBar == null ||
                _progressBarTrackFill == null)
            {
                return;
            }

            await ViewTransitionManager.Instance.Play(
                _progressBar,
                ProgressBarEnterClass);

            await ViewTransitionManager.Instance.Play(
                _progressBarTrackFill,
                ProgressBarTrackFillEnterClass);
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

        private async Awaitable PlayCardDeckIntroAnimation()
        {
            if (_cardDeck == null)
            {
                return;
            }

            await ViewTransitionManager.Instance.Play(
                _cardDeck,
                CardDeckEnterClass);
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
