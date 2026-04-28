using System;
using System.Reflection;
using Game.Core.Define;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager
    {
        public static Action<EDisplayAspect> ViewAspectChanged;

        public string[] GetAspectPresetLabels()
        {
            EDisplayAspect[] presets = (EDisplayAspect[])Enum.GetValues(typeof(EDisplayAspect));
            string[] labels = new string[presets.Length];
            for (int i = 0; i < presets.Length; i++)
            {
                labels[i] = GetAspectPresetLabel(presets[i]);
            }

            return labels;
        }

        public EDisplayAspect GetAspectPreset()
        {
            return _settings.Aspect;
        }
        
        public void SetAspectPreset(EDisplayAspect preset)
        {
            if (_settings.Aspect == preset)
            {
                return;
            }

            _settings.Aspect = preset;
            ViewAspectChanged?.Invoke(preset);
        }

        public string GetAspectPresetText()
        {
            return GetAspectPresetLabel(_settings.Aspect);
        }

        public string GetAspectPresetLabel(EDisplayAspect preset)
        {
            FieldInfo field = typeof(EDisplayAspect).GetField(preset.ToString());
            if (field != null && field.GetCustomAttribute<DisplayNameAttribute>() is DisplayNameAttribute attribute)
            {
                return attribute.DisplayName;
            }

            return preset.ToString();
        }

        public bool TryGetAspectPresetAtIndex(int index, out EDisplayAspect preset)
        {
            EDisplayAspect[] presets = (EDisplayAspect[])Enum.GetValues(typeof(EDisplayAspect));
            if (index < 0 || index >= presets.Length)
            {
                preset = default;
                return false;
            }

            preset = presets[index];
            return true;
        }
    }
}
