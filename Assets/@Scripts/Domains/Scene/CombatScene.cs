using Domains.Combat;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using UnityEngine;

namespace Domains.Scene
{
    public sealed class CombatScene : BaseScene
    {
        protected override async Awaitable OnBeforeUnload()
        {
            await Awaitable.NextFrameAsync();
        }

        protected override void OnLoaded()
        {
            CombatView combatView = DependencyManager.Instance.Instantiate<CombatView>();
            ViewManager.Instance.Push(combatView);
        }

        protected override void OnUnloaded()
        {
        }
    }
}
