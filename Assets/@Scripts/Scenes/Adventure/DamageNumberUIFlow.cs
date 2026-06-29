using System;
using Domains.Adventure;
using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Resolves the card body position for a damage cue and asks the manager to present the number.
    public sealed class DamageNumberUIFlow
    {
        private readonly AdventureBoardCardRegistry _cardRegistry;
        private readonly DamageNumberManager _damageNumberManager;
        private VisualElement _effectLayer;

        public DamageNumberUIFlow(
            AdventureBoardCardRegistry cardRegistry,
            DamageNumberManager damageNumberManager)
        {
            _cardRegistry = cardRegistry ?? throw new ArgumentNullException(nameof(cardRegistry));
            _damageNumberManager = damageNumberManager ?? throw new ArgumentNullException(nameof(damageNumberManager));
        }

        public void Bind(VisualElement effectLayer)
        {
            _effectLayer = effectLayer ?? throw new ArgumentNullException(nameof(effectLayer));
        }

        public async Awaitable Play(
            uint cardId,
            DamageCueData data)
        {
            if (_effectLayer == null || data == null || data.Amount <= 0)
                return;

            if (!_cardRegistry.TryGetCardElement(cardId, out VisualElement cardElement))
                return;

            CardWidget cardBody = cardElement.Q<CardWidget>();
            if (cardBody == null)
                return;

            Vector2 position = _effectLayer.WorldToLocal(cardBody.worldBound.center);
            await _damageNumberManager.Play(_effectLayer, data.Amount, position);
        }

        public void Unbind()
        {
            _damageNumberManager.Clear();
            _effectLayer = null;
        }
    }
}
