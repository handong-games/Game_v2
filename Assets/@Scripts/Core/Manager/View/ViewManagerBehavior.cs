using UnityEngine;

namespace Game.Core.Managers.View
{
    public sealed class ViewManagerBehavior : MonoBehaviour
    {
        private ViewManager _viewManager;
        private int _lastWidth;
        private int _lastHeight;

        public void Initialize(ViewManager viewManager)
        {
            _viewManager = viewManager;
        }

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

                _viewManager?.OnViewportSizeChanged(currentWidth, currentHeight);
            }
        }
    }
}
