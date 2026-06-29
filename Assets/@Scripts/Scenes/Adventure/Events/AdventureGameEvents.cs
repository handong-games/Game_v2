using System;
using Game.Scenes.Adventure.Events.Flow;

namespace Game.Scenes.Adventure.Events
{
    // Role:
    // Groups game-flow output event channels used by AdventureScene.
    // It does not represent widget input events.
    public sealed class AdventureGameEvents
    {
        public AdventureScreenEvents Screen { get; }
        public AdventureBoardEvents Board { get; }
        public AdventureCombatEvents Combat { get; }

        public AdventureGameEvents(
            AdventureScreenEvents screen,
            AdventureBoardEvents board,
            AdventureCombatEvents combat)
        {
            Screen = screen ?? throw new ArgumentNullException(nameof(screen));
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Combat = combat ?? throw new ArgumentNullException(nameof(combat));
        }
    }
}
