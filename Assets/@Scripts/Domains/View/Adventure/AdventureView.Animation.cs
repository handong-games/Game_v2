using Domains.Combat;
using Domains.Player;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed partial class AdventureView
    {
        private const string AdventureIntroShownClass = "adventure-view--intro-shown";
        private const int TurnBannerExitStartMs = 1200;

        private async Awaitable PlayIntroAnimation()
        {
            // Reset Animation
            _adventureRoot?.RemoveFromClassList(AdventureIntroShownClass);

            Awaitable introCompletion = WaitForIntroCompletion();

            if (_banner != null)
            {
                _ = _banner.PresentRegion(
                    _entryPresentation.Subtitle,
                    _entryPresentation.Title);
            }

            await Awaitable.NextFrameAsync();

            _boardUIFlow.PlaceCards(_controller.GetBoardCards());
            RegisterCardEvents();
            _adventureRoot?.AddToClassList(AdventureIntroShownClass);
            await introCompletion;

            if (_entryPresentation == null)
                return;

            BindGameplayCueReceivers();
            _controller.OnInitialBoardShown();
        }

        private async Awaitable PlayTurnBannerAnimation()
        {
            CombatTurnViewModel turn = _controller.GetCombatTurnViewModel();
            ViewTransitionTimeline timeline = new ViewTransitionTimeline();

            timeline
                .Run(0, () => _banner.PresentPlayerTurn(turn.TurnNumber))
                .Run(TurnBannerExitStartMs, _pouch.Show);

            await _viewTransitionManager.Play(timeline);
        }

        private Awaitable PlayEnemyTurnBannerAnimation()
        {
            return _banner.PresentEnemyTurn();
        }

        private async Awaitable PlayCoinFlipAsync(CoinFlipCueData data)
        {
            if (data == null)
                return;

            await _coinEffectPlayer.Play(
                data,
                _pouch,
                _coinStatusWidget.GetTarget(ECoinFace.Heads),
                _coinStatusWidget.GetTarget(ECoinFace.Tails),
                _coinStatusWidget.Add);

            await ShowSkillSlots();
            await _endTurnWidget.Show();
        }

        private async Awaitable PlayCoinChangeAsync(CoinChangeCueData data)
        {
            if (data == null || !data.HasEntries)
                return;

            await _coinStatusWidget.Show();
            await _coinChangeEffectPlayer.Play(
                data,
                (face, delta) => _coinStatusWidget.ApplyDelta(face, delta));
        }

        private Awaitable WaitForIntroCompletion()
        {
            if (_adventureRoot == null || _cardDeck == null)
            {
                return Awaitable.NextFrameAsync();
            }

            AwaitableCompletionSource completionSource = new();
            bool completed = false;
            EventCallback<TransitionEndEvent> onTransitionEnd = null;

            void Complete()
            {
                if (completed)
                    return;

                completed = true;
                _cardDeck.UnregisterCallback(onTransitionEnd);
                completionSource.SetResult();
            }

            onTransitionEnd = evt =>
            {
                if (evt.target != _cardDeck)
                    return;

                Complete();
            };

            _cardDeck.RegisterCallback(onTransitionEnd);
            return completionSource.Awaitable;
        }
    }
}
