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

        /*private void LateUpdate()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            _lastDisplayInfo = Screen.mainWindowDisplayInfo;
            _lastWindowPosition = Screen.mainWindowPosition;
        }*/
    }
}
