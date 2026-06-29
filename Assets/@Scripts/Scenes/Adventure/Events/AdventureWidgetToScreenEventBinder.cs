using System;
using Domains.Adventure;
using Game.Scenes.Adventure.Events.Widgets;

namespace Game.Scenes.Adventure.Events
{
    // Role:
    // Connects widget-originated input events to the Adventure screen.
    // This keeps user input routing separate from game-flow presentation events.
    public sealed class AdventureWidgetToScreenEventBinder : IDisposable
    {
        private readonly AdventureWidgetEvents _widgetEvents;
        private readonly AdventureView _view;
        private bool _started;

        public AdventureWidgetToScreenEventBinder(AdventureWidgetEvents widgetEvents, AdventureView view)
        {
            _widgetEvents = widgetEvents ?? throw new ArgumentNullException(nameof(widgetEvents));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Bind()
        {
            if (_started)
                return;

            _started = true;
            _widgetEvents.Turn.EndTurnClicked += _view.OnWidgetEndTurnClicked;
            _widgetEvents.Pouch.Clicked += _view.OnWidgetPouchClicked;
            _widgetEvents.Card.PointerEntered += _view.OnWidgetCardPointerEntered;
            _widgetEvents.Card.PointerLeft += _view.OnWidgetCardPointerLeft;
            _widgetEvents.Card.Clicked += _view.OnWidgetCardClicked;
            _widgetEvents.SkillSlot.SelectionChanged += _view.OnWidgetSkillSlotSelectionChanged;
        }

        public void Dispose()
        {
            if (!_started)
                return;

            _started = false;
            _widgetEvents.Turn.EndTurnClicked -= _view.OnWidgetEndTurnClicked;
            _widgetEvents.Pouch.Clicked -= _view.OnWidgetPouchClicked;
            _widgetEvents.Card.PointerEntered -= _view.OnWidgetCardPointerEntered;
            _widgetEvents.Card.PointerLeft -= _view.OnWidgetCardPointerLeft;
            _widgetEvents.Card.Clicked -= _view.OnWidgetCardClicked;
            _widgetEvents.SkillSlot.SelectionChanged -= _view.OnWidgetSkillSlotSelectionChanged;
        }
    }
}
