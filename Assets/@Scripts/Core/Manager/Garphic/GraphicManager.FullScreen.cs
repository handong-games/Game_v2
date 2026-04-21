using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager
    {
        public void SetFullscreen(bool fullscreen)
        {
            if (_saveData.fullscreen == fullscreen)
                return;

            _saveData.fullscreen = !_saveData.fullscreen;

            if (fullscreen)
            {
                /* 현재 윈도우 크기 저장 */
                _saveData.windowedWidth = Screen.width;
                _saveData.windowedHeight = Screen.height;

                /* 현재 윈도우 위치 저장 */
                Vector2Int position = Screen.mainWindowPosition;
                _saveData.windowPositionX = position.x;
                _saveData.windowPositionY = position.y;

                /* 전체화면 적용 */
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }
            else
            {
                /* 저장된 윈도우 크기 설정 */
                Screen.SetResolution(_saveData.windowedWidth, _saveData.windowedHeight, _saveData.fullscreen);

                /* 저장된 윈도우 위치 설정 */
                Vector2Int savedPosition = new Vector2Int(_saveData.windowPositionX, _saveData.windowPositionY);
                Screen.MoveMainWindowTo(Screen.mainWindowDisplayInfo, savedPosition);
            }
        }

        public bool IsFullscreen()
        {
            return _saveData.fullscreen;
        }
    }
}
