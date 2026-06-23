using System;
using Domains.Adventure;

namespace Domains.Scene.Adventure
{
    public sealed class AdventureViewEventBinder : IDisposable
    {
        private readonly AdventureEvents _events;
        private readonly AdventureWidgetEvents _widgetEvents;
        private readonly AdventureView _view;

        public AdventureViewEventBinder(
            AdventureEvents events,
            AdventureWidgetEvents widgetEvents,
            AdventureView view)
        {
            _events = events;
            _widgetEvents = widgetEvents;
            _view = view;

            Bind();
        }

        private void Bind()
        {
            _events.Board.RefreshRequested += _view.OnBoardChanged;
            _events.Combat.PlayerTurnBannerRequested += _view.OnTurnBannerRequested;
            _events.Combat.EnemyTurnBannerRequested += _view.OnEnemyTurnBannerRequested;
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
            _events.Combat.EnemyTurnBannerRequested -= _view.OnEnemyTurnBannerRequested;
            _events.Combat.ResultRequested -= _view.OnCombatEnded;
            
            // Widget -> View
            _widgetEvents.Turn.EndTurnClicked -= _view.OnEndTurnClicked;
            _widgetEvents.Pouch.Clicked -= _view.OnPouchClicked;
            _widgetEvents.SkillSlot.SelectionChanged -= _view.OnSkillSlotSelectionChanged;
        }
    }
}
