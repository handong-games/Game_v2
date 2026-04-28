using Domains.CharacterSelect;
using Domains.Settings;
using Domains.Settings.View;
using Game.Core.Managers.Audio;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using Views.TitleView;

namespace Domains.Scene.TitleScene
{
    public class TitleScene : BaseScene
    {
        private const string TitleMenuBgmAddress = "TitleMenuBgm";
        private AsyncOperationHandle _preloadOperation;

        protected override void OnLoaded()
        {
            /* Localization */
            _preloadOperation = LocalizationSettings.StringDatabase.PreloadTables(
                new TableReference[]
                {
                    nameof(TitleView),
                    nameof(CharacterSelectView),
                    nameof(SettingsView),
                    "CharacterNames",
                }
            );

            _preloadOperation.WaitForCompletion();

            /* Audio */
            var clip = Addressables.LoadAssetAsync<UnityEngine.AudioClip>(TitleMenuBgmAddress).WaitForCompletion();
            AudioManager.Instance.Play(EAudioPlay.BGM, clip);
            
            /* View */
            TitleView titleView = DependencyManager.Instance.Instantiate<TitleView>();
            ViewManager.Instance.Push(titleView);
        }

        protected override void OnUnloaded()
        {
            _preloadOperation.Release();
        }
    }
}
