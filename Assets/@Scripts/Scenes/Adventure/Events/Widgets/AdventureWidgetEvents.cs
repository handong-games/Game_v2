using System;

namespace Game.Scenes.Adventure.Events.Widgets
{
    // Role:
    // Groups widget-originated input event channels used by AdventureScreen.
    // It does not carry game-flow presentation requests.
    public sealed class AdventureWidgetEvents
    {
        public AdventureTurnWidgetEvents Turn { get; }
        public AdventurePouchWidgetEvents Pouch { get; }
        public AdventureCardWidgetEvents Card { get; }
        public AdventureSkillSlotWidgetEvents SkillSlot { get; }

        public AdventureWidgetEvents(
            AdventureTurnWidgetEvents turn,
            AdventurePouchWidgetEvents pouch,
            AdventureCardWidgetEvents card,
            AdventureSkillSlotWidgetEvents skillSlot)
        {
            Turn = turn ?? throw new ArgumentNullException(nameof(turn));
            Pouch = pouch ?? throw new ArgumentNullException(nameof(pouch));
            Card = card ?? throw new ArgumentNullException(nameof(card));
            SkillSlot = skillSlot ?? throw new ArgumentNullException(nameof(skillSlot));
        }
    }
}
