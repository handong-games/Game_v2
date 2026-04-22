using System.Collections.Generic;
using System;
using Game.Core.Managers.Save;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView : BaseView
    {
        private const string ActiveTabButtonClass = "settings-screen__tab-button--active";
        private const string ActiveTabPanelClass = "settings-screen__tab-panel--active";

        private VisualElement _screenRoot;
        private Button _tabGeneral;
        private Button _tabGraphics;
        private Button _tabAudio;
        private VisualElement _generalPanel;
        private VisualElement _graphicsPanel;
        private VisualElement _audioPanel;
        private Button _closeButton;
        private Action _onGeneralTabClicked;
        private Action _onGraphicsTabClicked;
        private Action _onAudioTabClicked;

        private readonly List<Button> _tabButtons = new();
        private readonly List<VisualElement> _tabPanels = new();

        protected override void OnBind(VisualElement root)
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

            UpdateRootLayerState();
            SelectTab(1);
        }

        public override void Dispose()
        {
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
        }

        private void SelectTab(int index)
        {
            for (int i = 0; i < _tabButtons.Count; i++)
                _tabButtons[i]?.EnableInClassList(ActiveTabButtonClass, i == index);

            for (int i = 0; i < _tabPanels.Count; i++)
                _tabPanels[i]?.EnableInClassList(ActiveTabPanelClass, i == index);
        }

        private void OnClose()
        {
            SettingManager.Instance.Save();
            ViewManager.Instance.Pop();
        }

        private void UpdateRootLayerState()
        {
            VisualElement rootLayer = ViewManager.Instance.RootLayer;
            if (rootLayer == null)
                return;

            rootLayer.EnableInClassList("app--fullscreen", false);
            rootLayer.EnableInClassList("app--windowed", true);
            rootLayer.EnableInClassList("app--single-display", Mathf.Max(Display.displays.Length, 1) <= 1);
            rootLayer.EnableInClassList("app--multi-display", Mathf.Max(Display.displays.Length, 1) > 1);
        }

        private void UpdateFullscreenVisualState(bool isFullscreen)
        {
            _screenRoot.EnableInClassList("settings-screen--fullscreen", isFullscreen);
            _screenRoot.EnableInClassList("settings-screen--windowed", !isFullscreen);
        }
    }
}
