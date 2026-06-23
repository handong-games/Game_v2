using System;
using System.Collections.Generic;
using Game.Core.Managers.View;
using Game.Core.Ports;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView : BaseView
    {
        private const string ActiveTabButtonClass = "settings-screen__tab-button--active";
        private const string ActiveTabPanelClass = "settings-screen__tab-panel--active";
        private const string IntroShownClass = "settings-screen--intro-shown";
        private const string ClosingClass = "settings-screen--closing";
        private const int DefaultTabIndex = 1;

        private VisualElement _screenRoot;
        private Button _tabGeneral;
        private Button _tabGraphics;
        private Button _tabAudio;
        private VisualElement _generalPanel;
        private VisualElement _graphicsPanel;
        private VisualElement _audioPanel;
        private Button _closeButton;
        private readonly SettingsViewController _controller;
        private readonly IViewHost _viewHost;
        private Action _onGeneralTabClicked;
        private Action _onGraphicsTabClicked;
        private Action _onAudioTabClicked;

        private readonly List<Button> _tabButtons = new();
        private readonly List<VisualElement> _tabPanels = new();
        private int _selectedTabIndex = DefaultTabIndex;
        private bool _isClosing;

        public SettingsView(
            SettingsViewController controller,
            IViewHost viewHost)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _viewHost = viewHost ?? throw new ArgumentNullException(nameof(viewHost));
        }

        protected override void OnVisualTreeCloned(VisualElement root)
        {
            if (Root.childCount == 0)
                return;

            _screenRoot = Root.Q<VisualElement>("settings-root");
            _tabGeneral = Root.Q<Button>("tab-general");
            _tabGraphics = Root.Q<Button>("tab-graphics");
            _tabAudio = Root.Q<Button>("tab-audio");
            _generalPanel = Root.Q<VisualElement>("general-tab-content");
            _graphicsPanel = Root.Q<VisualElement>("graphics-tab-content");
            _audioPanel = Root.Q<VisualElement>("audio-tab-content");
            _closeButton = Root.Q<Button>("settings-close-button");

            _screenRoot.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
            _closeButton.clicked += OnClose;
            _onGeneralTabClicked = () => SelectTab(0);
            _onGraphicsTabClicked = () => SelectTab(1);
            _onAudioTabClicked = () => SelectTab(2);
            _tabGeneral.clicked += _onGeneralTabClicked;
            _tabGraphics.clicked += _onGraphicsTabClicked;
            _tabAudio.clicked += _onAudioTabClicked;
            
            if (_tabGeneral != null)
                _tabButtons.Add(_tabGeneral);
            if (_tabGraphics != null)
                _tabButtons.Add(_tabGraphics);
            if (_tabAudio != null)
                _tabButtons.Add(_tabAudio);

            if (_generalPanel != null)
                _tabPanels.Add(_generalPanel);
            if (_graphicsPanel != null)
                _tabPanels.Add(_graphicsPanel);
            if (_audioPanel != null)
                _tabPanels.Add(_audioPanel);
            
            OnBindGeneral();
            OnBindGraphics();
            OnBindAudio();
        }

        protected override void OnShown()
        {
            ResetCloseState();
            RefreshGeneral();
            RefreshGraphics();
            RefreshAudio();
            UpdateRootLayerState();
            SelectTab(_selectedTabIndex);
            ResetIntroState();
            _ = PlayIntroAnimation();
        }

        protected override void OnHidden()
        {
            ResetIntroState();
        }

        public override void Dispose()
        {
            if (_screenRoot != null)
            {
                _screenRoot.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
            }

            OnUnbindGeneral();
            OnUnbindGraphics();
            OnUnbindAudio();

            if (_closeButton != null)
                _closeButton.clicked -= OnClose;

            if (_tabGeneral != null && _onGeneralTabClicked != null)
                _tabGeneral.clicked -= _onGeneralTabClicked;
            if (_tabGraphics != null && _onGraphicsTabClicked != null)
                _tabGraphics.clicked -= _onGraphicsTabClicked;
            if (_tabAudio != null && _onAudioTabClicked != null)
                _tabAudio.clicked -= _onAudioTabClicked;

            base.Dispose();
        }

        private void SelectTab(int index)
        {
            int maxIndex = Math.Min(_tabButtons.Count, _tabPanels.Count) - 1;
            if (maxIndex < 0)
                return;

            _selectedTabIndex = Math.Max(0, Math.Min(index, maxIndex));

            for (int i = 0; i < _tabButtons.Count; i++)
                _tabButtons[i]?.EnableInClassList(ActiveTabButtonClass, i == _selectedTabIndex);

            for (int i = 0; i < _tabPanels.Count; i++)
                _tabPanels[i]?.EnableInClassList(ActiveTabPanelClass, i == _selectedTabIndex);
        }

        private void OnClose()
        {
            if (_isClosing)
                return;

            PlayCloseAnimation();
        }

        private void OnCloseAnimationCompleted()
        {
            _isClosing = false;
            _controller?.OnClose();
        }

        private void UpdateRootLayerState()
        {
            VisualElement rootLayer = _viewHost.RootLayer;
            if (rootLayer == null)
                return;

            int displayCount = Mathf.Max(Display.displays.Length, 1);
            rootLayer.EnableInClassList("app--fullscreen", false);
            rootLayer.EnableInClassList("app--windowed", true);
            rootLayer.EnableInClassList("app--single-display", displayCount <= 1);
            rootLayer.EnableInClassList("app--multi-display", displayCount > 1);
        }
    }
}
