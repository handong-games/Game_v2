using Domains.CharacterSelect;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using Domains.Scene.TitleScene;
using Domains.Settings.View;
using Game.Core.Managers.Save;
using UnityEngine;

namespace Views.TitleView
{
    [Dependency(nameof(TitleScene))]
    public class TitleViewController
    {
        public void OnNewGame()
        {
            CharacterSelectView view = DependencyManager.Instance.Instantiate<CharacterSelectView>();
            ViewManager.Instance.Push(view);
        }

        public void OnSettings()
        {
            SettingsView view = DependencyManager.Instance.Instantiate<SettingsView>();
            ViewManager.Instance.Push(view);
        }
        
        public void OnQuit()
        {
            Application.Quit(0);
        }
    }
}
