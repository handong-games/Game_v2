using System.Collections.Generic;
using Domains.Combat;
using Domains.Intent.Flow;
using Domains.Intent.Presentation;
using Game.Scenes.Adventure.Events.Flow;
using Gameplay.GAS;
using System;
using UnityEngine;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Executes skill activation and skill target confirmation.
    // It validates game-side target rules but does not own screen-local targeting UI state.
    public sealed class AdventureSkillFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventurePlayer _player;
        private readonly AdventureCards _cards;
        private readonly AdventureCombatRuntime _combat;
        private readonly IntentRefreshFlow _intentRefreshFlow;
        private readonly IntentPresenter _intentPresenter;
        private readonly AdventureCombatEvents _combatEvents;

        public AdventureSkillFlow(
            AdventureProgress progress,
            AdventurePlayer player,
            AdventureCards cards,
            AdventureCombatRuntime combat,
            IntentRefreshFlow intentRefreshFlow,
            IntentPresenter intentPresenter,
            AdventureCombatEvents combatEvents)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _intentRefreshFlow = intentRefreshFlow ?? throw new ArgumentNullException(nameof(intentRefreshFlow));
            _intentPresenter = intentPresenter ?? throw new ArgumentNullException(nameof(intentPresenter));
            _combatEvents = combatEvents ?? throw new ArgumentNullException(nameof(combatEvents));
        }

        public async Awaitable<bool> UseSkill(GameplayAbilitySpecHandle handle)
        {
            if (!CanUseSkill(handle))
                return false;

            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return false;

            bool activated = playerCard.AbilitySystem.TryActivateAbility(handle);
            if (!activated)
                return false;

            await RefreshVisibleEnemyIntents();
            return true;
        }

        public async Awaitable<bool> UseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            if (!CanUseSkillOnTarget(handle, targetCardId))
                return false;

            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return false;

            if (!_cards.TryGet(targetCardId, out CardActor targetCard))
                return false;

            GameplayEventData eventData = new(default)
            {
                Instigator = playerCard.AbilitySystem,
                Target = targetCard.AbilitySystem,
            };

            bool result = playerCard.AbilitySystem.TriggerAbilityFromGameplayEvent(handle, eventData);
            if (result)
                await RefreshVisibleEnemyIntents();

            return result;
        }

        public bool CanUseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            if (!CanUseSkill(handle))
                return false;

            if (_player.PlayerCard == null)
                return false;

            if (!_cards.TryGet(targetCardId, out CardActor targetCard) ||
                targetCard?.AbilitySystem == null)
            {
                return false;
            }

            return _combat.TryGetSide(targetCardId, out ECombatSide side) &&
                   side == ECombatSide.Enemy;
        }

        public bool CanUseSkill(GameplayAbilitySpecHandle handle)
        {
            return handle.IsValid &&
                   _progress.CurrentPhase == AdventurePhase.Combat &&
                   !_combat.IsEnded &&
                   _combat.CurrentSide == ECombatSide.Player;
        }

        private async Awaitable RefreshVisibleEnemyIntents()
        {
            if (_combat.IsEnded)
                return;

            IReadOnlyList<uint> enemyCardIds = _combat.EnemyCardIds;
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                uint enemyCardId = enemyCardIds[i];
                if (!_intentRefreshFlow.Refresh(enemyCardId))
                    continue;

                MonsterIntentRevealViewModel viewModel = _intentPresenter.Create(enemyCardId);

                if (_combatEvents.IntentRefreshRequested == null)
                    throw new InvalidOperationException(
                        $"{nameof(AdventureCombatEvents.IntentRefreshRequested)} is not bound.");

                await _combatEvents.IntentRefreshRequested.Invoke(viewModel);
            }
        }
    }
}
