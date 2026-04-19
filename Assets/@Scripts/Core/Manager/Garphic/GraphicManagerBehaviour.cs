using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public sealed class GraphicManagerBehaviour : MonoBehaviour
    {
        private int _lastWidth;
        private int _lastHeight;
        private DisplayInfo _lastDisplayInfo;
        private Vector2Int _lastWindowPosition;

        private void Awake()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _lastDisplayInfo = Screen.mainWindowDisplayInfo;
            _lastWindowPosition = Screen.mainWindowPosition;
        }

        private void LateUpdate()
        {
            DisplayInfo currentDisplayInfo = Screen.mainWindowDisplayInfo;
            Vector2Int currentWindowPosition = Screen.mainWindowPosition;
            bool sizeChanged = _lastWidth != Screen.width || _lastHeight != Screen.height;
            bool displayChanged =
                _lastDisplayInfo.name != currentDisplayInfo.name ||
                _lastDisplayInfo.width != currentDisplayInfo.width ||
                _lastDisplayInfo.height != currentDisplayInfo.height;
            bool windowPositionChanged = _lastWindowPosition != currentWindowPosition;

            if (!sizeChanged && !displayChanged && !windowPositionChanged)
            {
                return;
            }

            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _lastDisplayInfo = currentDisplayInfo;
            _lastWindowPosition = currentWindowPosition;

            if (sizeChanged)
            {
                GraphicManager.Instance.OnWindowSizeChanged(_lastWidth, _lastHeight);
            }

            if (displayChanged)
            {
                GraphicManager.Instance.OnDisplayChanged();
            }

            if (windowPositionChanged)
            {
                GraphicManager.Instance.OnWindowPositionChanged(_lastWindowPosition);
            }
        }
    }
}
