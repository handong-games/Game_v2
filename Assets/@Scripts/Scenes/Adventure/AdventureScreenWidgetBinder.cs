using System;
using System.Collections.Generic;
using Domains.View.Widgets;
using Game.Scenes.Adventure.Events.Widgets;

namespace Game.Scenes.Adventure
{
    // Role:
    // Binds Adventure screen widgets to widget-originated event channels.
    // AdventureView owns screen lifecycle, but this class owns widget event wiring.
    public sealed class AdventureScreenWidgetBinder : IDisposable
    {
        private readonly AdventureWidgetEvents _events;
        private readonly AdventureScreenWidgetTemplates _templates;

        private AdventureScreenWidgets _widgets;
        private bool _bound;

        public AdventureScreenWidgetBinder(
            AdventureWidgetEvents events,
            AdventureScreenWidgetTemplates templates)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        public void Bind(
            AdventureScreenWidgets widgets,
            IReadOnlyList<AdventureSkillSlotViewModel> skillSlots)
        {
            if (widgets == null)
                throw new ArgumentNullException(nameof(widgets));

            if (skillSlots == null)
                throw new ArgumentNullException(nameof(skillSlots));

            Unbind();
            EnsureWidgetEventsBound();

            _widgets = widgets;
            _widgets.EndTurn.Bind(_events.Turn);
            _widgets.Pouch.Bind(_events.Pouch);
            _widgets.SkillSlots.Bind(skillSlots, _events.SkillSlot, _templates);
            _bound = true;
        }

        public void Unbind()
        {
            if (!_bound || _widgets == null)
                return;

            _widgets.Pouch.Unbind();
            _widgets.EndTurn.Unbind();
            _widgets.SkillSlots.Unbind();
            _widgets = null;
            _bound = false;
        }

        public void Dispose()
        {
            Unbind();
        }

        private void EnsureWidgetEventsBound()
        {
            if (_events.Turn.EndTurnClicked == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureTurnWidgetEvents.EndTurnClicked)} is not bound.");

            if (_events.Pouch.Clicked == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventurePouchWidgetEvents.Clicked)} is not bound.");

            if (_events.SkillSlot.SelectionChanged == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureSkillSlotWidgetEvents.SelectionChanged)} is not bound.");
        }
    }
}
