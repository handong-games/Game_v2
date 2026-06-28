using Game.Scenes.Adventure.Events.Flow;

namespace Game.Scenes.Adventure.Events
{
    public sealed class AdventureGameEvents
    {
        public AdventureBoardEvents Board { get; }
        public AdventureCombatEvents Combat { get; }

        public AdventureGameEvents(
            AdventureBoardEvents board,
            AdventureCombatEvents combat)
        {
            Board = board;
            Combat = combat;
        }
    }
}
