using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager
    {
        private readonly List<Vector2Int> _resolutionOptions = new();
        
        public void SetResolution(int width, int height)
        {
            if (IsFullscreen())
            {
                return;
            }
            
            if (_saveData.windowedWidth == width && _saveData.windowedHeight == height)
            {
                return;
            }

            _saveData.windowedWidth = width;
            _saveData.windowedHeight = height;
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }
        
        public IReadOnlyList<Vector2Int> GetResolutions()
        {
            _resolutionOptions.Clear();

            Resolution[] resolutions = Screen.resolutions;
            HashSet<(int width, int height)> seen = new HashSet<(int width, int height)>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution resolution = resolutions[i];
                if (!seen.Add((resolution.width, resolution.height)))
                {
                    continue;
                }

                _resolutionOptions.Add(new Vector2Int(resolution.width, resolution.height));
            }
            
            return _resolutionOptions;
        }

        public string GetCurrentResolutionText()
        {
            if (Screen.fullScreen)
            {
                return "N/A";
            }

            return $"{Screen.width} x {Screen.height}";
        }
    }
}
