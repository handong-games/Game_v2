using Domains.Adventure;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Domains.Scene
{
    public sealed class AdventureScene : BaseScene
    {
        private static readonly TableReference[] LocalizationTables =
        {
            nameof(AdventureView),
            "CharacterNames",
            "MonsterNames",
            "SkillNames",
        };

        protected override void OnLoaded()
        {
            /* Localization */
            AsyncOperationHandle preloadOperation = LocalizationSettings.StringDatabase.PreloadTables(
                LocalizationTables);

            preloadOperation.WaitForCompletion();

            /* View */
            AdventureView adventureView = DependencyManager.Instance.Instantiate<AdventureView>();
            ViewManager.Instance.Push(adventureView);

            AdventureController adventureController = DependencyManager.Instance.Resolve<AdventureController>();
            adventureController.StartAdventure();
        }
        
        protected override async Awaitable OnBeforeUnload()
        {
            await Awaitable.NextFrameAsync();
        }

        protected override void OnUnloaded()
        {
            /* Localization */
            for (int i = 0; i < LocalizationTables.Length; i++)
            {
                LocalizationSettings.StringDatabase.ReleaseTable(LocalizationTables[i]);
            }
        }
    }
}
