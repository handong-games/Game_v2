using Domains.CharacterSelect;
using Domains.Settings;
using Domains.Settings.View;
using Game.Core.Managers.Audio;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using Views.TitleView;

namespace Domains.Scene
{
    public class TitleScene : BaseScene
    {
        private const string TitleMenuBgmAddress = "TitleMenuBgm";

        protected override void OnLoaded()
        {
            /* Localization */
            AsyncOperationHandle preloadOperation = LocalizationSettings.StringDatabase.PreloadTables(
                new TableReference[]
                {
                    nameof(TitleView),
                    nameof(CharacterSelectView),
                    nameof(SettingsView),
                    "CharacterNames",
                }
            );

            preloadOperation.WaitForCompletion();

            /* Audio */
            var clip = Addressables.LoadAssetAsync<UnityEngine.AudioClip>(TitleMenuBgmAddress).WaitForCompletion();
            AudioManager.Instance.Play(EAudioPlay.BGM, clip);
            
            /* View */
            TitleView titleView = DependencyManager.Instance.Instantiate<TitleView>();
            ViewManager.Instance.Push(titleView);
        }

        protected override async Awaitable OnBeforeUnload()
        {
            /* View Transition */
            VisualElement overlayLayer = ViewManager.Instance.OverlayLayer;
            overlayLayer.pickingMode = PickingMode.Position;

            await ViewTransitionManager.Instance.Play(overlayLayer, EViewTransitionType.FadeOut);

            overlayLayer.pickingMode = PickingMode.Ignore;
        }

        protected override void OnUnloaded()
        {
            /* Localization */
            LocalizationSettings.StringDatabase.ReleaseTable(nameof(TitleView));
            LocalizationSettings.StringDatabase.ReleaseTable(nameof(CharacterSelectView));
            LocalizationSettings.StringDatabase.ReleaseTable(nameof(SettingsView));

            /* Audio */
        }
    }
}
