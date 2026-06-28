using System;
using Domains.Adventure;
using Game.Scenes.Adventure.Events.Widgets;
using VContainer.Unity;

namespace Game.Scenes.Adventure.Events
{
    public sealed class AdventureViewEventBinder : IStartable, IDisposable
    {
        private readonly AdventureGameEvents _events;
        private readonly AdventureWidgetEvents _widgetEvents;
        private readonly AdventureView _view;

        public AdventureViewEventBinder(
            AdventureGameEvents events,
            AdventureWidgetEvents widgetEvents,
            AdventureView view)
        {
            _events = events;
            _widgetEvents = widgetEvents;
            _view = view;
        }

        public void Start()
        {
            _events.Board.RefreshRequested += _view.OnBoardChanged;
            _events.Combat.PlayerTurnBannerRequested += _view.OnTurnBannerRequested;
            _events.Combat.IntentRevealRequested += _view.OnIntentRevealRequested;
            _events.Combat.IntentTriggeredRequested += _view.OnIntentTriggeredRequested;
            _events.Combat.EnemyTurnCompleted += _view.OnEnemyTurnCompleted;
            _events.Combat.ResultRequested += _view.OnCombatEnded;
            
            // Widget -> View
            _widgetEvents.Turn.EndTurnClicked += _view.OnEndTurnClicked;
            _widgetEvents.Pouch.Clicked += _view.OnPouchClicked;
            _widgetEvents.SkillSlot.SelectionChanged += _view.OnSkillSlotSelectionChanged;
        }

        public void Dispose()
        {
            _events.Board.RefreshRequested -= _view.OnBoardChanged;
            _events.Combat.PlayerTurnBannerRequested -= _view.OnTurnBannerRequested;
            _events.Combat.IntentRevealRequested -= _view.OnIntentRevealRequested;
            _events.Combat.IntentTriggeredRequested -= _view.OnIntentTriggeredRequested;
            _events.Combat.EnemyTurnCompleted -= _view.OnEnemyTurnCompleted;
            _events.Combat.ResultRequested -= _view.OnCombatEnded;
            
            // Widget -> View
            _widgetEvents.Turn.EndTurnClicked -= _view.OnEndTurnClicked;
            _widgetEvents.Pouch.Clicked -= _view.OnPouchClicked;
            _widgetEvents.SkillSlot.SelectionChanged -= _view.OnSkillSlotSelectionChanged;
        }
    }
}
