using Domains.Scene.Title;
using UnityEngine;

namespace Views.TitleView
{
    public class TitleViewController
    {
        private readonly TitleSceneNavigator _navigator;

        public TitleViewController(TitleSceneNavigator navigator)
        {
            _navigator = navigator;
        }

        public void OnNewGame()
        {
            _ = _navigator.ShowCharacterSelect();
        }

        public void OnSettings()
        {
            _navigator.ShowSettings();
        }

        public void OnQuit()
        {
            Application.Quit(0);
        }
    }
}
