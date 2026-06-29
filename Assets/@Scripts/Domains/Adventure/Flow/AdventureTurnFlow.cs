using Game.AbilitySystem;
using Game.Scenes.Adventure.Events.Flow;
using Gameplay.GAS;
using Domains.Intent.Flow;
using Domains.Combat;
using System;
using UnityEngine;
using IntentPresenter = Domains.Intent.Presentation.IntentPresenter;

namespace Domains.Adventure
{
    // Role:
    // Coordinates player/enemy turn transitions for Adventure combat.
    // It does not execute enemy actions by itself.
    public sealed class AdventureTurnFlow
    {
        private readonly AdventureInputState _input;
        private readonly AdventureProgress _progress;
        private readonly AdventurePlayer _player;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCoinFlow _coinFlow;
        private readonly AdventureEnemyActionFlow _enemyActionFlow;
        private readonly IntentPrepareFlow _intentPrepareFlow;
        private readonly IntentPresenter _intentPresenter;
        private readonly AdventureScreenEvents _screenEvents;

        public AdventureTurnFlow(
            AdventureInputState input,
            AdventureProgress progress,
            AdventurePlayer player,
            AdventureCombatRuntime combat,
            AdventureCoinFlow coinFlow,
            AdventureEnemyActionFlow enemyActionFlow,
            IntentPrepareFlow intentPrepareFlow,
            IntentPresenter intentPresenter,
            AdventureScreenEvents screenEvents)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _coinFlow = coinFlow ?? throw new ArgumentNullException(nameof(coinFlow));
            _enemyActionFlow = enemyActionFlow ?? throw new ArgumentNullException(nameof(enemyActionFlow));
            _intentPrepareFlow = intentPrepareFlow ?? throw new ArgumentNullException(nameof(intentPrepareFlow));
            _intentPresenter = intentPresenter ?? throw new ArgumentNullException(nameof(intentPresenter));
            _screenEvents = screenEvents ?? throw new ArgumentNullException(nameof(screenEvents));
        }

        public async Awaitable<bool> StartInitialPlayerTurn()
        {
            if (_combat.IsEnded)
                return false;

            return await RevealPlayerTurnIntents();
        }

        public void EndPlayerTurn()
        {
            if (!CanEndPlayerTurn())
                return;

            _coinFlow.ResetCoin();
            _input.Clear();

            AbilitySystemComponent abilitySystem = _player.PlayerCard?.AbilitySystem;
            if (abilitySystem == null)
                return;

            abilitySystem.HandleGameplayEvent(new GameplayEventData(AbilityGameplayTags.EventCombatTurnEnded)
            {
                Instigator = abilitySystem,
            });
        }

        public async Awaitable EndPlayerTurnAndStartEnemyTurn()
        {
            if (!CanEndPlayerTurn())
                return;

            EndPlayerTurn();

            if (_combat.IsEnded)
                return;

            if (_screenEvents.EnemyTurnStarted == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureScreenEvents.EnemyTurnStarted)} is not bound.");

            bool enemyTurnPresentationCompleted =
                await _screenEvents.EnemyTurnStarted.Invoke(
                    new AdventureEnemyTurnStartViewModel(
                        new CombatTurnViewModel(
                            ECombatSide.Enemy,
                            _combat.RoundNumber)));

            if (!enemyTurnPresentationCompleted)
                return;

            if (_combat.IsEnded)
                return;

            await StartEnemyTurn();
        }

        public bool CanEndPlayerTurn()
        {
            return _progress.CurrentPhase == AdventurePhase.Combat &&
                   !_combat.IsEnded &&
                   _combat.CurrentSide == ECombatSide.Player;
        }

        public async Awaitable StartEnemyTurn()
        {
            _combat.StartEnemyTurn();
            await _enemyActionFlow.ExecuteEnemyTurn();

            if (_combat.IsEnded)
                return;

            await CompleteEnemyTurn();
        }

        public async Awaitable<bool> CompleteEnemyTurn()
        {
            if (_combat.IsEnded)
                return false;

            _combat.StartPlayerTurn();
            return await RevealPlayerTurnIntents();
        }

        private async Awaitable<bool> RevealPlayerTurnIntents()
        {
            if (_combat.IsEnded)
                return false;

            _intentPrepareFlow.PrepareAll();

            if (_screenEvents.PlayerTurnStarted == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureScreenEvents.PlayerTurnStarted)} is not bound.");

            return await _screenEvents.PlayerTurnStarted.Invoke(
                new AdventurePlayerTurnStartViewModel(
                    new CombatTurnViewModel(
                        _combat.CurrentSide,
                        _combat.RoundNumber),
                    _intentPresenter.CreateRevealSequence()));
        }
    }
}
