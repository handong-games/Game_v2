using VContainer;
using VContainer.Unity;

namespace Game.Core.Composition
{
    public sealed class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Phase 1 skeleton only. Scene-scoped controllers are documented,
            // but not registered until the runtime scene bridge is introduced.
        }
    }
}
