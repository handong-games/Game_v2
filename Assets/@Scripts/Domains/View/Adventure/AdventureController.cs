using Domains.Scene;
using Game.Core.Managers.Dependency;

namespace Domains.Adventure
{
    [Dependency(nameof(AdventureScene))]
    public sealed class AdventureController
    {
        [Inject]
        private AdventureService _adventureService;
    }
}
