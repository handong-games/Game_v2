using Game.Core.Managers.View;
using Domains.View.Widgets;
using Game.Scenes.Adventure;
using System;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    // Role:
    // Owns the Adventure screen lifecycle and delegates UI sequences, board creation, and commands.
    // It is the only Adventure UI object that talks directly to AdventureScreenController.
    public sealed partial class AdventureView :
        BaseView,
        IAdventureCoinFlipCueReceiver,
        IAdventureCoinChangeCueReceiver
    {
        private readonly AdventureScreenController _controller;
        private readonly AdventureScreenWidgets _screenWidgets;
        private readonly AdventureScreenWidgetBinder _screenWidgetBinder;
        private readonly AdventureBoardUIFlow _boardUIFlow;
        private readonly AdventureScreenUIFlow _screenUIFlow;
        private readonly AdventureSkillUIFlow _skillUIFlow;
        private readonly AdventureGameplayCueAvatarBinder _gameplayCueAvatarBinder;

        private bool _initialPresentationPrepared;
        private bool _screenAttached;
        private bool _introStarted;
        private bool _widgetCommandRunning;
        private int _screenLifetimeVersion;
        private bool _isDisposed;

        public AdventureView(
            AdventureScreenController controller,
            AdventureScreenWidgets screenWidgets,
            AdventureScreenWidgetBinder screenWidgetBinder,
            AdventureBoardUIFlow boardUIFlow,
            AdventureScreenUIFlow screenUIFlow,
            AdventureSkillUIFlow skillUIFlow,
            AdventureGameplayCueAvatarBinder gameplayCueAvatarBinder)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _screenWidgets = screenWidgets ?? throw new ArgumentNullException(nameof(screenWidgets));
            _screenWidgetBinder = screenWidgetBinder ?? throw new ArgumentNullException(nameof(screenWidgetBinder));
            _boardUIFlow = boardUIFlow ?? throw new ArgumentNullException(nameof(boardUIFlow));
            _screenUIFlow = screenUIFlow ?? throw new ArgumentNullException(nameof(screenUIFlow));
            _skillUIFlow = skillUIFlow ?? throw new ArgumentNullException(nameof(skillUIFlow));
            _gameplayCueAvatarBinder = gameplayCueAvatarBinder ?? throw new ArgumentNullException(nameof(gameplayCueAvatarBinder));
        }

        protected override void OnVisualTreeCloned(VisualElement root)
        {
            _isDisposed = false;
        }

        protected override void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            base.OnAttachedToPanel(evt);

            if (!_screenWidgets.IsInitialized)
            {
                _screenWidgets.Initialize(LogicalRoot);
                _screenUIFlow.Bind(_screenWidgets.EffectLayer);
            }

            _screenAttached = true;
        }

        protected override void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            base.OnDetachedFromPanel(evt);

            _screenAttached = false;
            _screenLifetimeVersion++;
            ClearScreenRuntimeBindings();
            ResetScreenPresentationState();
        }

        private async UnityEngine.Awaitable<bool> WaitUntilScreenReady(int version)
        {
            while (IsCurrentScreenLifetime(version) &&
                   (!_screenWidgets.IsInitialized || !_screenAttached))
            {
                await UnityEngine.Awaitable.NextFrameAsync();
            }

            return IsCurrentScreenLifetime(version);
        }

        private void PrepareInitialPresentation(AdventureInitialPresentationViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (_initialPresentationPrepared)
                return;

            if (!_screenWidgets.IsInitialized)
                throw new InvalidOperationException("Adventure screen widgets are not initialized.");

            _screenWidgetBinder.Bind(_screenWidgets, viewModel.SkillSlots);
            _skillUIFlow.Bind(
                viewModel.SkillSlots,
                _screenWidgets.SkillSlots,
                _screenWidgets.AdventureRoot,
                _screenWidgets.Arrow);

            _initialPresentationPrepared = true;
        }

        private bool IsCurrentScreenLifetime(int version)
        {
            return !_isDisposed && _screenLifetimeVersion == version;
        }

        public override void Dispose()
        {
            _isDisposed = true;
            _screenLifetimeVersion++;

            ClearScreenRuntimeBindings();
            ResetScreenPresentationState();
            base.Dispose();
        }

        private void ClearScreenRuntimeBindings()
        {
            UnbindGameplayCueReceivers();
            _skillUIFlow.Unbind();
            ClearCards();
            _screenWidgetBinder.Unbind();

            _screenUIFlow.Unbind();
            _screenWidgets.Clear();
        }

        private void ResetScreenPresentationState()
        {
            _initialPresentationPrepared = false;
            _introStarted = false;
            _screenAttached = false;
            _widgetCommandRunning = false;
        }

        private bool TryBeginWidgetCommand()
        {
            if (_widgetCommandRunning)
                return false;

            _widgetCommandRunning = true;
            return true;
        }

        private void EndWidgetCommand()
        {
            _widgetCommandRunning = false;
        }

        private static void LogAsyncException(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }
}
