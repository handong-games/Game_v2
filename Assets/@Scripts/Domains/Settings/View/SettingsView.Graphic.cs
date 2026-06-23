using System.Collections.Generic;
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
            
            _aspectRatioField = Bind<DropdownField, string>("aspect-ratio-field", OnAspectRatioChanged);
            
            _resolutionField = Bind<DropdownField, string>("resolution-field", OnResolutionSelected);
            _resolutionField.RegisterCallback<PointerDownEvent>(OnResolutionDropdownPointerDown);
        }

        private void RefreshGraphics()
        {
            bool isFullscreen = _controller.IsFullscreen();
            _fullscreenToggle.SetValueWithoutNotify(isFullscreen);

            _aspectRatioField.choices = new List<string>(_controller.GetAspectPresetLabels());
            _aspectRatioField.SetValueWithoutNotify(_controller.GetAspectPresetText());

            _resolutionField.SetValueWithoutNotify(_controller.GetCurrentResolutionText());
            _resolutionField.SetEnabled(!isFullscreen);
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
            return new List<string>(_controller.GetResolutionLabels());
        }

        private string GetCurrentResolutionText()
        {
            return _controller.GetCurrentResolutionText();
        }

        private void OnResolutionDropdownPointerDown(PointerDownEvent evt)
        {
            _resolutionField.choices = GetResolutions();
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }

        private void OnFullscreenChanged(ChangeEvent<bool> evt)
        {
            _controller.SetFullscreen(evt.newValue);
            bool isFullscreen = _controller.IsFullscreen();
            
            _resolutionField.SetEnabled(!isFullscreen);
            _resolutionField.SetValueWithoutNotify(_controller.GetCurrentResolutionText());
        }

        private void OnResolutionSelected(ChangeEvent<string> evt)
        {
            if (!_controller.SetResolutionAtIndex(_resolutionField.index))
                return;
            
            _resolutionField.SetValueWithoutNotify(_controller.GetCurrentResolutionText());
        }

        private void OnAspectRatioChanged(ChangeEvent<string> evt)
        {
            if (!_controller.SetAspectPresetAtIndex(_aspectRatioField.index))
                return;

            UpdateRootLayerState();
            _resolutionField.SetValueWithoutNotify(GetCurrentResolutionText());
        }
    }
}
