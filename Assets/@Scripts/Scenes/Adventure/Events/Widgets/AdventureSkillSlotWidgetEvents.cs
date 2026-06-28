using System;
using Domains.View.Widgets;

namespace Game.Scenes.Adventure.Events.Widgets
{
    public sealed class AdventureSkillSlotWidgetEvents
    {
        public Action<int, SkillSlotWidget> SelectionChanged;
    }
}
