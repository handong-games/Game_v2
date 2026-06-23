using System;
using Domains.View.Widgets;

namespace Domains.Adventure
{
    public sealed class AdventureWidgetEvents
    {
        public AdventureTurnWidgetEvents Turn { get; }
        public AdventurePouchWidgetEvents Pouch { get; }
        public AdventureSkillSlotWidgetEvents SkillSlot { get; }

        public AdventureWidgetEvents(
            AdventureTurnWidgetEvents turn,
            AdventurePouchWidgetEvents pouch,
            AdventureSkillSlotWidgetEvents skillSlot)
        {
            Turn = turn;
            Pouch = pouch;
            SkillSlot = skillSlot;
        }
    }

    public sealed class AdventureTurnWidgetEvents
    {
        public Action EndTurnClicked;
    }

    public sealed class AdventurePouchWidgetEvents
    {
        public Action Clicked;
    }

    public sealed class AdventureSkillSlotWidgetEvents
    {
        public Action<int, SkillSlotWidget> SelectionChanged;
    }
}
