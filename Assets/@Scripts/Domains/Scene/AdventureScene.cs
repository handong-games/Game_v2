using Domains.Adventure;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using UnityEngine;

namespace Domains.Scene
{
    public sealed class AdventureScene : BaseScene
    {
        protected override void OnLoaded()
        {
            AdventureView adventureView = DependencyManager.Instance.Instantiate<AdventureView>();
            ViewManager.Instance.Push(adventureView);
        }
        
        protected override async Awaitable OnBeforeUnload()
        {
            await Awaitable.NextFrameAsync();
        }

        protected override void OnUnloaded()
        {
        }
    }
}
