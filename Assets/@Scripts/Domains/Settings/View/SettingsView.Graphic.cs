using System.Collections.Generic;
using Game.Core.Managers.Garphic;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView
    {
        private static readonly string[] AspectRatioChoices =
        {
            "Auto",
            "4:3",
            "16:9",
            "16:10",
            "21:9"
        };

        private Toggle _fullscreenToggle;
        private DropdownField _resolutionField;
        private DropdownField _aspectRatioField;

        private void OnBindGraphics()
        {
            _fullscreenToggle = Bind<Toggle, bool>("fullscreen-toggle", OnFullscreenChanged);
            _fullscreenToggle.SetValueWithoutNotify(GraphicManager.Instance.IsFullscreen());
            
            _aspectRatioField = Bind<DropdownField, string>("aspect-ratio-field", OnAspectRatioChanged);
            _aspectRatioField.SetValueWithoutNotify(GetAspectPresetText());
            _aspectRatioField.choices = new List<string>(AspectRatioChoices);
            
            _resolutionField = Bind<DropdownField, string>("resolution-field", OnResolutionSelected);
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
            _resolutionField.SetEnabled(!GraphicManager.Instance.IsFullscreen());
            _resolutionField.RegisterCallback<PointerDownEvent>(OnResolutionDropdownPointerDown);
            
            GraphicManager.Instance.WindowSizeChanged += OnWindowSizeChanged;
        }

        // 해상도 변경 이벤트
        private void OnWindowSizeChanged(int resolutionWidth, int resolutionHeight)
        {
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }

        private void OnUnbindGraphics()
        {
            GraphicManager.Instance.WindowSizeChanged -= OnWindowSizeChanged;
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
            return GraphicManager.Instance.IsFullscreen()
                ? "N/A"
                : $"{Screen.width} x {Screen.height}";
        }

        private string GetAspectPresetText()
        {
            return ViewManager.Instance.GetAspectPreset() switch
            {
                EDisplayAspectPreset.Auto => "Auto",
                EDisplayAspectPreset.Ratio4x3 => "4:3",
                EDisplayAspectPreset.Ratio16x9 => "16:9",
                EDisplayAspectPreset.Ratio16x10 => "16:10",
                EDisplayAspectPreset.Ratio21x9 => "21:9",
                _ => "Auto"
            };
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
            UpdateFullscreenVisualState(isFullscreen);
            UpdateSharedRootState();
            _resolutionField.SetEnabled(!isFullscreen);
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }

        private void OnResolutionSelected(ChangeEvent<string> evt)
        {
            int index = _resolutionField.index;
            IReadOnlyList<Vector2Int> resolutionList = GraphicManager.Instance.GetResolutions();
            if (index < 0 || index >= resolutionList.Count)
                return;

            Vector2Int resolution = resolutionList[index];
            GraphicManager.Instance.SetResolution(resolution.x, resolution.y);
        }

        private void OnAspectRatioChanged(ChangeEvent<string> evt)
        {
            int index = _aspectRatioField.index;
            if (index < 0 || index > (int)EDisplayAspectPreset.Auto)
                return;

            ViewManager.Instance.SetAspectPreset((EDisplayAspectPreset)index);
            UpdateSharedRootState();
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }

    }
}
