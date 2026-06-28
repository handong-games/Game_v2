using Game.AbilitySystem;
using Game.Scenes.Adventure.Events.Flow;
using Gameplay.GAS;
using Domains.Intent.Flow;
using IntentPresenter = Domains.Intent.Presentation.IntentPresenter;

namespace Domains.Adventure
{
    // Role:
    // Coordinates player/enemy turn transitions for Adventure combat.
    // It does not execute enemy actions by itself.
    public sealed class AdventureTurnFlow
    {
        private readonly AdventureInputState _input;
        private readonly AdventurePlayer _player;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCoinFlow _coinFlow;
        private readonly AdventureEnemyActionFlow _enemyActionFlow;
        private readonly IntentPrepareFlow _intentPrepareFlow;
        private readonly IntentPresenter _intentPresenter;
        private readonly AdventureCombatEvents _events;

        public AdventureTurnFlow(
            AdventureInputState input,
            AdventurePlayer player,
            AdventureCombatRuntime combat,
            AdventureCoinFlow coinFlow,
            AdventureEnemyActionFlow enemyActionFlow,
            IntentPrepareFlow intentPrepareFlow,
            IntentPresenter intentPresenter,
            AdventureCombatEvents events)
        {
            _input = input;
            _player = player;
            _combat = combat;
            _coinFlow = coinFlow;
            _enemyActionFlow = enemyActionFlow;
            _intentPrepareFlow = intentPrepareFlow;
            _intentPresenter = intentPresenter;
            _events = events;
        }

        public void StartInitialPlayerTurn()
        {
            RevealPlayerTurnIntents();
        }

        public void EndPlayerTurn()
        {
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

        public void StartEnemyTurn()
        {
            _combat.StartEnemyTurn();
            _enemyActionFlow.ExecuteEnemyTurn();
        }

        public void CompleteEnemyTurn()
        {
            _combat.StartPlayerTurn();
            RevealPlayerTurnIntents();
        }

        private void RevealPlayerTurnIntents()
        {
            _intentPrepareFlow.PrepareAll();
            _events.PlayerTurnBannerRequested?.Invoke();
            _events.IntentRevealRequested?.Invoke(_intentPresenter.CreateRevealSequence());
        }
    }
}
