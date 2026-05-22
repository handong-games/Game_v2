namespace Domains.Combat
{
    public readonly struct EndTurnResultDto
    {
        public EndTurnResultDto(int turnNumber)
        {
            TurnNumber = turnNumber;
        }

        public int TurnNumber { get; }
    }
}
