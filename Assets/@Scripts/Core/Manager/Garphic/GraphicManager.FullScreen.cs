using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager
    {
        public void SetFullscreen(bool fullscreen)
        {
            if (_settings.Fullscreen == fullscreen)
                return;

            _settings.Fullscreen = !_settings.Fullscreen;

            if (fullscreen)
            {
                /* 현재 윈도우 크기 저장 */
                _settings.WindowedWidth = Screen.width;
                _settings.WindowedHeight = Screen.height;

                /* 현재 윈도우 위치 저장 */
                Vector2Int position = Screen.mainWindowPosition;
                _settings.WindowPositionX = position.x;
                _settings.WindowPositionY = position.y;

                /* 전체화면 적용 */
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
            else
            {
                /* 저장된 윈도우 크기 설정 */
                Screen.SetResolution(_settings.WindowedWidth, _settings.WindowedHeight, _settings.Fullscreen);

                /* 저장된 윈도우 위치 설정 */
                Vector2Int savedPosition = new Vector2Int(_settings.WindowPositionX, _settings.WindowPositionY);
                Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, savedPosition);
            }
        }

        public bool IsFullscreen()
        {
            return _settings.Fullscreen;
        }
    }
}
