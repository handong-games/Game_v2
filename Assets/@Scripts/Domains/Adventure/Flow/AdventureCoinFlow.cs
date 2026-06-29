using System;
using Game.AbilitySystem;
using Gameplay.GAS;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Sends coin-related gameplay events to the player card AbilitySystem.
    // It does not update UI state directly.
    public sealed class AdventureCoinFlow
    {
        private readonly AdventurePlayer _player;
        private readonly AdventureProgress _progress;
        private readonly AdventureCombatRuntime _combat;

        public AdventureCoinFlow(
            AdventurePlayer player,
            AdventureProgress progress,
            AdventureCombatRuntime combat)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
        }

        public void FlipCoin()
        {
            if (!CanFlipCoin())
                return;

            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return;

            playerCard.AbilitySystem.HandleGameplayEvent(new GameplayEventData(AbilityGameplayTags.EventCoinFlip)
            {
                Instigator = playerCard.AbilitySystem,
            });
        }

        public bool CanFlipCoin()
        {
            return _progress.CurrentPhase == AdventurePhase.Combat &&
                   !_combat.IsEnded &&
                   _combat.CurrentSide == Domains.Combat.ECombatSide.Player;
        }

        public void ResetCoin()
        {
            // Role note:
            // Current project has no coin reset gameplay event.
            // Coin reset must be modeled explicitly before this flow mutates GE state.
        }
    }
}
