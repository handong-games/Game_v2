namespace Game.Scenes.Adventure.Events.Widgets
{
    public sealed class AdventureWidgetEvents
    {
        public AdventureTurnWidgetEvents Turn { get; }
        public AdventurePouchWidgetEvents Pouch { get; }
        public AdventureSkillSlotWidgetEvents SkillSlot { get; }
        public IntentBadgeWidgetEvents IntentBadge { get; }

        public AdventureWidgetEvents(
            AdventureTurnWidgetEvents turn,
            AdventurePouchWidgetEvents pouch,
            AdventureSkillSlotWidgetEvents skillSlot,
            IntentBadgeWidgetEvents intentBadge)
        {
            Turn = turn;
            Pouch = pouch;
            SkillSlot = skillSlot;
            IntentBadge = intentBadge;
        }
    }
}
