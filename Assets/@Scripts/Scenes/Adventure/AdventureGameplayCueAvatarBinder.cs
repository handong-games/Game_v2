using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.Card;
using Domains.View.Widgets;
using UnityEngine;

namespace Game.Scenes.Adventure
{
    // Role:
    // Builds screen-side avatar bindings for visible Adventure board cards.
    // It does not mutate AbilitySystem state; AdventureCardAvatarBindingFlow does that.
    public sealed class AdventureGameplayCueAvatarBinder
    {
        private readonly DamageNumberUIFlow _damageNumberUIFlow;

        public AdventureGameplayCueAvatarBinder(DamageNumberUIFlow damageNumberUIFlow)
        {
            _damageNumberUIFlow = damageNumberUIFlow ?? throw new ArgumentNullException(nameof(damageNumberUIFlow));
        }

        public IReadOnlyList<AdventureCardAvatarBinding> CreateBindings(
            IReadOnlyList<AdventureCardViewModel> cards,
            object screenReceiver,
            AdventureBoardUIFlow boardUIFlow)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            if (screenReceiver == null)
                throw new ArgumentNullException(nameof(screenReceiver));

            if (boardUIFlow == null)
                throw new ArgumentNullException(nameof(boardUIFlow));

            List<AdventureCardAvatarBinding> result = new();

            for (int i = 0; i < cards.Count; i++)
            {
                AdventureCardViewModel card = cards[i];
                if (card == null)
                    throw new InvalidOperationException("Adventure runtime card view model is missing.");

                if (card.AbilitySystem == null)
                    throw new InvalidOperationException(
                        $"Adventure runtime card ability system is missing: {card.CardId}");

                object receiver = card.Zone switch
                {
                    ECardZone.Left => new AdventureGameplayCueCompositeReceiver(
                        card.CardId,
                        screenReceiver,
                        FindCardReceiver(boardUIFlow, AdventureBoardSide.Left, card.CardId),
                        _damageNumberUIFlow),
                    ECardZone.Right => new AdventureGameplayCueCompositeReceiver(
                        card.CardId,
                        null,
                        FindCardReceiver(boardUIFlow, AdventureBoardSide.Right, card.CardId),
                        _damageNumberUIFlow),
                    _ => null,
                };

                if (receiver != null)
                    result.Add(new AdventureCardAvatarBinding(card.CardId, receiver));
            }

            return result;
        }

        private static IAdventureDamageCueReceiver FindCardReceiver(
            AdventureBoardUIFlow boardUIFlow,
            AdventureBoardSide side,
            uint cardId)
        {
            return boardUIFlow.TryGetCardWidget(
                side,
                cardId,
                out IAdventureDamageCueReceiver receiver)
                ? receiver
                : null;
        }

        private sealed class AdventureGameplayCueCompositeReceiver :
            IAdventureCoinFlipCueReceiver,
            IAdventureCoinChangeCueReceiver,
            IAdventureDamageCueReceiver
        {
            private readonly uint _cardId;
            private readonly IAdventureCoinFlipCueReceiver _coinFlipReceiver;
            private readonly IAdventureCoinChangeCueReceiver _coinChangeReceiver;
            private readonly IAdventureDamageCueReceiver _damageReceiver;
            private readonly DamageNumberUIFlow _damageNumberUIFlow;

            public AdventureGameplayCueCompositeReceiver(
                uint cardId,
                object screenReceiver,
                IAdventureDamageCueReceiver cardReceiver,
                DamageNumberUIFlow damageNumberUIFlow)
            {
                _cardId = cardId;
                _coinFlipReceiver = screenReceiver as IAdventureCoinFlipCueReceiver;
                _coinChangeReceiver = screenReceiver as IAdventureCoinChangeCueReceiver;
                _damageReceiver = cardReceiver;
                _damageNumberUIFlow = damageNumberUIFlow;
            }

            public void HandleCoinFlipCue(CoinFlipCueData data)
            {
                _coinFlipReceiver?.HandleCoinFlipCue(data);
            }

            public void HandleCoinChangeCue(CoinChangeCueData data)
            {
                _coinChangeReceiver?.HandleCoinChangeCue(data);
            }

            public void HandleDamageCue(DamageCueData data)
            {
                if (_damageReceiver == null)
                    return;

                _damageReceiver?.HandleDamageCue(data);
                _ = PlayDamageNumber(data);
            }

            private async Awaitable PlayDamageNumber(DamageCueData data)
            {
                if (_damageNumberUIFlow == null)
                    return;

                try
                {
                    await _damageNumberUIFlow.Play(_cardId, data);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}
