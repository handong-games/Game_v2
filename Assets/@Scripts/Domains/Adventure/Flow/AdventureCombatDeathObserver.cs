using System;
using System.Collections.Generic;
using Domains.Combat;
using Game.AbilitySystem;
using Game.Messages;
using VContainer.Unity;
using CardActor = Domains.Card.Card;

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
        private readonly AdventureCards _cards;
        private readonly AdventureCombatResultFlow _resultFlow;
        private IDisposable _subscription;

        public AdventureCombatDeathObserver(
            GameplayMessageManager messageManager,
            AdventureCardAvatarRegistry avatarRegistry,
            AdventureCombatRuntime combat,
            AdventureCards cards,
            AdventureCombatResultFlow resultFlow)
        {
            _messageManager = messageManager;
            _avatarRegistry = avatarRegistry;
            _combat = combat;
            _cards = cards;
            _resultFlow = resultFlow;
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

        private void OnCombatDeathMessage(GameplayDeathMessage message)
        {
            if (_combat.IsEnded || message.Avatar == null)
                return;

            if (!_avatarRegistry.TryGetCardId(message.Avatar, out uint cardId))
                return;

            if (!_combat.TryGetSide(cardId, out ECombatSide side))
                return;

            if (!_combat.MarkDeathResolved(cardId))
                return;

            if (side == ECombatSide.Player)
            {
                _resultFlow.CompleteCombat(ECombatEndResult.Defeat);
                return;
            }

            if (AreAllEnemiesDead())
                _resultFlow.CompleteCombat(ECombatEndResult.Victory);
        }

        private bool AreAllEnemiesDead()
        {
            IReadOnlyList<uint> enemyCardIds = _combat.EnemyCardIds;
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                if (!_cards.TryGet(enemyCardIds[i], out CardActor enemyCard))
                    continue;

                if (!enemyCard.AbilitySystem.OwnedTags.HasTagExact(StateGameplayTags.Dead))
                    return false;
            }

            return true;
        }
    }
}
