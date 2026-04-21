using System.Collections.Generic;
using Game.Core.Define;
using Game.Core.Managers.Garphic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView
    {
        private Toggle _fullscreenToggle;
        private DropdownField _resolutionField;
        private DropdownField _aspectRatioField;

        private void OnBindGraphics()
        {
            _fullscreenToggle = Bind<Toggle, bool>("fullscreen-toggle", OnFullscreenChanged);
            _fullscreenToggle.SetValueWithoutNotify(GraphicManager.Instance.IsFullscreen());
            
            _aspectRatioField = Bind<DropdownField, string>("aspect-ratio-field", OnAspectRatioChanged);
            _aspectRatioField.SetValueWithoutNotify(GraphicManager.Instance.GetAspectPresetText());
            _aspectRatioField.choices = new List<string>(GraphicManager.Instance.GetAspectPresetLabels());
            
            _resolutionField = Bind<DropdownField, string>("resolution-field", OnResolutionSelected);
            _resolutionField.SetValueWithoutNotify(GraphicManager.Instance.GetCurrentResolutionText());
            _resolutionField.SetEnabled(!GraphicManager.Instance.IsFullscreen());
            _resolutionField.RegisterCallback<PointerDownEvent>(OnResolutionDropdownPointerDown);
        }

        // 해상도 변경 이벤트
        private void OnWindowSizeChanged(int resolutionWidth, int resolutionHeight)
        {
            _resolutionField.SetValueWithoutNotify(GraphicManager.Instance.GetCurrentResolutionText());
        }

        private void OnUnbindGraphics()
        {
            _resolutionField?.UnregisterCallback<PointerDownEvent>(OnResolutionDropdownPointerDown);
            Unbind<Toggle, bool>(_fullscreenToggle, OnFullscreenChanged);
            Unbind<DropdownField, string>(_resolutionField, OnResolutionSelected);
            Unbind<DropdownField, string>(_aspectRatioField, OnAspectRatioChanged);
        }

        private List<string> GetResolutions()
        {
            IReadOnlyList<Vector2Int> resolutionList = GraphicManager.Instance.GetResolutions();
            List<string> labels = new List<string>(resolutionList.Count);

            for (int i = 0; i < resolutionList.Count; i++)
            {
                Vector2Int resolution = resolutionList[i];
                labels.Add($"{resolution.x} x {resolution.y}");
            }

            return labels;
        }

        private string GetCurrentResolutionText()
        {
            return GraphicManager.Instance.GetCurrentResolutionText();
        }

        private void OnResolutionDropdownPointerDown(PointerDownEvent evt)
        {
            _resolutionField.choices = GetResolutions();
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }

        private void OnFullscreenChanged(ChangeEvent<bool> evt)
        {
            GraphicManager.Instance.SetFullscreen(evt.newValue);
            bool isFullscreen = GraphicManager.Instance.IsFullscreen();
            //UpdateFullscreenVisualState(isFullscreen);
            //UpdateRootLayerState();
            
            _resolutionField.SetEnabled(!isFullscreen);
            _resolutionField.SetValueWithoutNotify(GraphicManager.Instance.GetCurrentResolutionText());
        }

        private void OnResolutionSelected(ChangeEvent<string> evt)
        {
            int index = _resolutionField.index;
            IReadOnlyList<Vector2Int> resolutionList = GraphicManager.Instance.GetResolutions();
            if (index < 0 || index >= resolutionList.Count)
            {
                return;
            }

            Vector2Int resolution = resolutionList[index];
            GraphicManager.Instance.SetResolution(resolution.x, resolution.y);
            _resolutionField.SetValueWithoutNotify(GraphicManager.Instance.GetCurrentResolutionText());
        }

        private void OnAspectRatioChanged(ChangeEvent<string> evt)
        {
            int index = _aspectRatioField.index;
            if (!GraphicManager.Instance.TryGetAspectPresetAtIndex(index, out EDisplayAspect preset))
            {
                return;
            }

            GraphicManager.Instance.SetAspectPreset(preset);
            UpdateRootLayerState();
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }
    }
}
