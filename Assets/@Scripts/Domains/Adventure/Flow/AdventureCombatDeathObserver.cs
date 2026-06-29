using System;
using Domains.Combat;
using Game.Messages;
using UnityEngine;
using VContainer.Unity;

namespace Domains.Adventure
{
    // Role:
    // Observes gameplay death messages and translates CardWidget avatars into combat results.
    // It owns message subscription lifetime, not combat state.
    public sealed class AdventureCombatDeathObserver : IStartable, IDisposable
    {
        private readonly GameplayMessageManager _messageManager;
        private readonly AdventureCardAvatarRegistry _avatarRegistry;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureEnemyDeathFlow _enemyDeathFlow;
        private readonly AdventureCombatResultFlow _resultFlow;
        private IDisposable _subscription;

        public AdventureCombatDeathObserver(
            GameplayMessageManager messageManager,
            AdventureCardAvatarRegistry avatarRegistry,
            AdventureCombatRuntime combat,
            AdventureEnemyDeathFlow enemyDeathFlow,
            AdventureCombatResultFlow resultFlow)
        {
            _messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
            _avatarRegistry = avatarRegistry ?? throw new ArgumentNullException(nameof(avatarRegistry));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _enemyDeathFlow = enemyDeathFlow ?? throw new ArgumentNullException(nameof(enemyDeathFlow));
            _resultFlow = resultFlow ?? throw new ArgumentNullException(nameof(resultFlow));
        }

        public void Start()
        {
            _subscription = _messageManager.Subscribe<GameplayDeathMessage>(
                GameplayMessageTags.CombatDeath,
                OnCombatDeathMessage);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private async void OnCombatDeathMessage(GameplayDeathMessage message)
        {
            try
            {
                if (_combat.IsEnded || message.Avatar == null)
                    return;

                if (!_avatarRegistry.TryGetCardId(message.Avatar, out uint cardId))
                    return;

                if (!_combat.TryGetSide(cardId, out ECombatSide side))
                    return;

                if (side == ECombatSide.Player)
                {
                    if (!_combat.MarkDeathResolved(cardId))
                        return;

                    await _resultFlow.CompleteCombat(ECombatEndResult.Defeat);
                    return;
                }

                await _enemyDeathFlow.HandleEnemyDeath(cardId);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
