using Domains.Run;
using Domains.Scene;
using Game.Core.Managers.Dependency;

namespace Domains.Combat
{
    [Dependency(nameof(CombatScene))]
    public sealed class CombatController
    {
        [Inject]
        private RunService _runService;
    }
}
