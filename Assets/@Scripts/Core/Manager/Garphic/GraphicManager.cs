using System;
using System.Collections.Generic;
using Domains.Settings;
using Game.Core.Managers.Save;
using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public class GraphicManager : BaseManager<GraphicManager>
    {
        public event Action<int, int> WindowSizeChanged;

        private GraphicSaveData _saveData;
        private GameObject _managerObject;
        private List<Vector2Int> _cachedResolutions;
        
        protected override void OnInit()
        {
            _saveData = SettingManager.Instance.SaveData.GraphicSaveData;

            _managerObject = new GameObject("GraphicManager");
            UnityEngine.Object.DontDestroyOnLoad(_managerObject);
            _managerObject.AddComponent<GraphicManagerBehaviour>();
        }

        protected override void OnDispose()
        {
            if (_managerObject != null)
            {
                UnityEngine.Object.Destroy(_managerObject);
                _managerObject = null;
            }

            _cachedResolutions = null;
            _saveData = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            if (Instance == null)
            {
                return;
            }

            Instance.ApplyDisplayMode();
        }

        public void OnWindowSizeChanged(int resolutionWidth, int resolutionHeight)
        {
            if (_saveData.fullscreen)
            {
                WindowSizeChanged?.Invoke(resolutionWidth, resolutionHeight);
                return;
            }

            _saveData.windowedWidth = resolutionWidth;
            _saveData.windowedHeight = resolutionHeight;
            WindowSizeChanged?.Invoke(resolutionWidth, resolutionHeight);
        }

        public void OnWindowPositionChanged(Vector2Int windowPosition)
        {
            UpdateWindowPosition(windowPosition);
        }

        public void OnDisplayChanged()
        {
            UpdateCurrentDisplayIndex();
            _cachedResolutions = null;
        }

        public void SetFullscreen(bool fullscreen)
        {
            if (_saveData.fullscreen == fullscreen)
                return;

            _saveData.fullscreen = fullscreen;
            ApplyDisplayMode();
        }

        public void SetResolution(int width, int height)
        {
            if (_saveData.fullscreen)
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

        private Vector2Int GetDisplayBoundsSize(int displayIndex)
        {
            List<DisplayInfo> displays = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displays);

            if (displays.Count == 0)
            {
                return new Vector2Int(
                    Mathf.Max(Screen.currentResolution.width, 1),
                    Mathf.Max(Screen.currentResolution.height, 1));
            }

            int normalizedIndex = Mathf.Clamp(displayIndex, 0, displays.Count - 1);
            DisplayInfo display = displays[normalizedIndex];
            return new Vector2Int(
                Mathf.Max(display.width, 1),
                Mathf.Max(display.height, 1));
        }

        private void ApplyDisplayMode()
        {
            if (_saveData.fullscreen)
            {
                UpdateCurrentDisplayIndex();

                Vector2Int fullscreenSize = GetDisplayBoundsSize(_saveData.targetDisplayIndex);
                Screen.SetResolution(fullscreenSize.x, fullscreenSize.y, FullScreenMode.FullScreenWindow);
                return;
            }
            
            Screen.SetResolution(_saveData.windowedWidth, _saveData.windowedHeight, FullScreenMode.Windowed);
            UpdateWindowedPosition();
        }

        private void UpdateCurrentDisplayIndex()
        {
            List<DisplayInfo> displays = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displays);

            if (displays.Count == 0)
            {
                return;
            }

            DisplayInfo currentDisplay = Screen.mainWindowDisplayInfo;
            for (int i = 0; i < displays.Count; i++)
            {
                if (displays[i].name == currentDisplay.name &&
                    displays[i].width == currentDisplay.width &&
                    displays[i].height == currentDisplay.height &&
                    displays[i].workArea.x == currentDisplay.workArea.x &&
                    displays[i].workArea.y == currentDisplay.workArea.y)
                {
                    _saveData.targetDisplayIndex = i;
                    return;
                }
            }
        }

        private void UpdateWindowPosition(Vector2Int currentPosition)
        {
            if (_saveData.fullscreen)
            {
                return;
            }
            
            _saveData.windowPositionX = currentPosition.x;
            _saveData.windowPositionY = currentPosition.y;
        }

        private void UpdateWindowedPosition()
        {
            if (_saveData.fullscreen)
            {
                return;
            }

            List<DisplayInfo> displays = new List<DisplayInfo>();
            Screen.GetDisplayLayout(displays);

            if (displays.Count == 0)
            {
                return;
            }

            int displayIndex = Mathf.Clamp(_saveData.targetDisplayIndex, 0, displays.Count - 1);
            DisplayInfo targetDisplay = displays[displayIndex];
            Vector2Int restorePosition = _saveData.windowPositionX >= 0 && _saveData.windowPositionY >= 0
                ? new Vector2Int(_saveData.windowPositionX, _saveData.windowPositionY)
                : new Vector2Int(8, 48);

            Screen.MoveMainWindowTo(targetDisplay, restorePosition);
        }

        public IReadOnlyList<Vector2Int> GetResolutions()
        {
            if (_cachedResolutions != null)
            {
                return _cachedResolutions;
            }

            _cachedResolutions = BuildResolutionsCache();
            return _cachedResolutions;
        }

        public bool IsFullscreen()
        {
            return _saveData.fullscreen;
        }

        private List<Vector2Int> BuildResolutionsCache()
        {
            Resolution[] resolutions = Screen.resolutions;
            HashSet<(int width, int height)> seen = new HashSet<(int width, int height)>();
            List<Vector2Int> result = new List<Vector2Int>(resolutions.Length);

            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution resolution = resolutions[i];
                if (!seen.Add((resolution.width, resolution.height)))
                {
                    continue;
                }

                result.Add(new Vector2Int(resolution.width, resolution.height));
            }

            result.Sort((left, right) =>
            {
                int widthCompare = left.x.CompareTo(right.x);
                return widthCompare != 0 ? widthCompare : left.y.CompareTo(right.y);
            });

            return result;
        }
    }
}
