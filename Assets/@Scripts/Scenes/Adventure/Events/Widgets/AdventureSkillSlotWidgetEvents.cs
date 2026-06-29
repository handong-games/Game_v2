using System;
using Domains.View.Widgets;

namespace Game.Scenes.Adventure.Events.Widgets
{
    // Role:
    // Carries skill-slot widget input from Adventure UI widgets to the Adventure screen.
    public sealed class AdventureSkillSlotWidgetEvents
    {
        public Action<int, SkillSlotWidget> SelectionChanged;
    }
}
