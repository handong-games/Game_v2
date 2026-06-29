using System;
using Domains.Adventure;
using Domains.Player;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Owns coin presentation: coin status visibility, coin flip effects, and coin value changes.
    // It does not execute coin gameplay or decide turn phase transitions.
    public sealed class AdventureCoinUIFlow
    {
        private readonly CoinEffectPlayer _coinEffectPlayer = new();
        private readonly CoinChangeEffectPlayer _coinChangeEffectPlayer = new();
        private readonly ViewTransitionManager _viewTransitionManager;

        public AdventureCoinUIFlow(ViewTransitionManager viewTransitionManager)
        {
            _viewTransitionManager = viewTransitionManager ?? throw new ArgumentNullException(nameof(viewTransitionManager));
        }

        public void Bind(VisualElement effectLayer)
        {
            if (effectLayer == null)
                throw new ArgumentNullException(nameof(effectLayer));

            _coinEffectPlayer.Bind(effectLayer);
        }

        public Awaitable ShowStatus(CoinStatusWidget coinStatus)
        {
            if (coinStatus == null)
                throw new ArgumentNullException(nameof(coinStatus));

            return coinStatus.Show(_viewTransitionManager);
        }

        public async Awaitable PlayCoinFlip(
            CoinFlipCueData data,
            Pouch pouch,
            CoinStatusWidget coinStatus)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (pouch == null)
                throw new ArgumentNullException(nameof(pouch));

            if (coinStatus == null)
                throw new ArgumentNullException(nameof(coinStatus));

            await _coinEffectPlayer.Play(
                data,
                pouch,
                coinStatus.GetTarget(ECoinFace.Heads),
                coinStatus.GetTarget(ECoinFace.Tails),
                coinStatus.Add);
        }

        public async Awaitable PlayCoinChange(
            CoinChangeCueData data,
            CoinStatusWidget coinStatus,
            Func<bool> canContinue)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (coinStatus == null)
                throw new ArgumentNullException(nameof(coinStatus));

            if (!data.HasEntries)
                return;

            await coinStatus.Show(_viewTransitionManager);
            if (canContinue != null && !canContinue.Invoke())
                return;

            await _coinChangeEffectPlayer.Play(
                data,
                (face, delta) => coinStatus.ApplyDelta(face, delta));
        }

        public void Clear()
        {
            _coinEffectPlayer.Clear();
            _coinChangeEffectPlayer.Clear();
        }

        public void Unbind()
        {
            _coinEffectPlayer.Unbind();
            _coinChangeEffectPlayer.Clear();
        }
    }
}
