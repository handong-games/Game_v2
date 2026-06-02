namespace Domains.Combat
{
    public readonly struct CombatTurnViewModel
    {
        public CombatTurnViewModel(ECombatSide side, int turnNumber)
        {
            Side = side;
            TurnNumber = turnNumber;
        }

        public ECombatSide Side { get; }
        public int TurnNumber { get; }
    }
}
