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

        public AdventureCoinFlow(AdventurePlayer player)
        {
            _player = player;
        }

        public void FlipCoin()
        {
            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return;

            playerCard.AbilitySystem.HandleGameplayEvent(new GameplayEventData(AbilityGameplayTags.EventCoinFlip)
            {
                Instigator = playerCard.AbilitySystem,
            });
        }

        public void ResetCoin()
        {
            // Role note:
            // Current project has no coin reset gameplay event.
            // Coin reset must be modeled explicitly before this flow mutates GE state.
        }
    }
}
