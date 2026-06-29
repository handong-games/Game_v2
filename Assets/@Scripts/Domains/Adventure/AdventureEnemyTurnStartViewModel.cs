using Domains.Combat;

namespace Domains.Adventure
{
    // Role:
    // Carries presentation data needed when the Adventure screen enters enemy turn.
    // Game flow prepares the data; UI flow decides how to present it.
    public sealed class AdventureEnemyTurnStartViewModel
    {
        public AdventureEnemyTurnStartViewModel(CombatTurnViewModel turn)
        {
            Turn = turn;
        }

        public CombatTurnViewModel Turn { get; }
    }
}
