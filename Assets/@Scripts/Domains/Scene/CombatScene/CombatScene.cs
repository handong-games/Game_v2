using Game.Core.Managers.Scene;

namespace Domains.Scene.CombatScene
{
    public sealed class CombatScene : BaseScene
    {
        protected override bool RequiresPreloadAssets => false;

        protected override void OnLoaded()
        {
        }

        protected override void OnUnloaded()
        {
        }
    }
}
