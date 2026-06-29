using System;
using Domains.Adventure;

namespace Game.Scenes.Adventure.Events
{
    // Role:
    // Connects game-flow events to the Adventure screen.
    // This keeps controller/flow output separate from widget input events.
    public sealed class AdventureGameToScreenEventBinder : IDisposable
    {
        private readonly AdventureGameEvents _events;
        private readonly AdventureView _view;
        private bool _started;

        public AdventureGameToScreenEventBinder(AdventureGameEvents events, AdventureView view)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Bind()
        {
            if (_started)
                return;

            _started = true;
            _events.Screen.InitialPresentationPrepared = _view.OnGameInitialPresentationPrepared;
            _events.Screen.PlayerTurnStarted = _view.OnGamePlayerTurnStarted;
            _events.Screen.EnemyTurnStarted = _view.OnGameEnemyTurnStarted;
            _events.Screen.RewardStarted = _view.OnGameRewardStarted;
            _events.Screen.ChoiceRefreshStarted = _view.OnGameChoiceRefreshStarted;
            _events.Board.RefreshRequested = _view.OnGameBoardRefreshRequested;
            _events.Board.CardRemoveRequested = _view.OnGameBoardCardRemoveRequested;
            _events.Board.CardDeathRequested = _view.OnGameBoardCardDeathRequested;
            _events.Board.SideClearRequested = _view.OnGameBoardSideClearRequested;
            _events.Combat.IntentRefreshRequested = _view.OnGameIntentRefreshRequested;
            _events.Combat.IntentTriggeredRequested = _view.OnGameIntentTriggeredRequested;
            _events.Combat.ResultRequested = _view.OnGameCombatEnded;
        }

        public void Dispose()
        {
            if (!_started)
                return;

            _started = false;
            if (_events.Screen.InitialPresentationPrepared == _view.OnGameInitialPresentationPrepared)
                _events.Screen.InitialPresentationPrepared = null;

            if (_events.Screen.PlayerTurnStarted == _view.OnGamePlayerTurnStarted)
                _events.Screen.PlayerTurnStarted = null;

            if (_events.Screen.EnemyTurnStarted == _view.OnGameEnemyTurnStarted)
                _events.Screen.EnemyTurnStarted = null;

            if (_events.Screen.RewardStarted == _view.OnGameRewardStarted)
                _events.Screen.RewardStarted = null;

            if (_events.Screen.ChoiceRefreshStarted == _view.OnGameChoiceRefreshStarted)
                _events.Screen.ChoiceRefreshStarted = null;

            if (_events.Board.RefreshRequested == _view.OnGameBoardRefreshRequested)
                _events.Board.RefreshRequested = null;

            if (_events.Board.CardRemoveRequested == _view.OnGameBoardCardRemoveRequested)
                _events.Board.CardRemoveRequested = null;

            if (_events.Board.CardDeathRequested == _view.OnGameBoardCardDeathRequested)
                _events.Board.CardDeathRequested = null;

            if (_events.Board.SideClearRequested == _view.OnGameBoardSideClearRequested)
                _events.Board.SideClearRequested = null;

            if (_events.Combat.IntentRefreshRequested == _view.OnGameIntentRefreshRequested)
                _events.Combat.IntentRefreshRequested = null;

            if (_events.Combat.IntentTriggeredRequested == _view.OnGameIntentTriggeredRequested)
                _events.Combat.IntentTriggeredRequested = null;

            if (_events.Combat.ResultRequested == _view.OnGameCombatEnded)
                _events.Combat.ResultRequested = null;
        }
    }
}
