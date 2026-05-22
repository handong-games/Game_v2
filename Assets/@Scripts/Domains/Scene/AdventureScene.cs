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
        private const string MonsterNamesTable = "MonsterNames";

        protected override void OnLoaded()
        {
            /* Localization */
            AsyncOperationHandle preloadOperation = LocalizationSettings.StringDatabase.PreloadTables(
                new TableReference[]
                {
                    MonsterNamesTable,
                }
            );

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
            LocalizationSettings.StringDatabase.ReleaseTable(MonsterNamesTable);
        }
    }
}
