using UnityEngine;

namespace Game.Core.Managers.View
{
    public sealed class ViewManagerBehavior : MonoBehaviour
    {
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
        }

        private void LateUpdate()
        {
            int currentWidth = Screen.width;
            int currentHeight = Screen.height;

            if (_lastWidth != currentWidth || _lastHeight != currentHeight)
            {
                _lastWidth = currentWidth;
                _lastHeight = currentHeight;

                ViewManager.Instance.OnViewportSizeChanged(currentWidth, currentHeight);
            }
        }
    }
}
