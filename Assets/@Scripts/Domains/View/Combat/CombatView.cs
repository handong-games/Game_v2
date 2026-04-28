using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Domains.Combat
{
    public sealed class CombatView : BaseView
    {
        [Inject]
        private CombatController _controller;

        protected override void OnBind(VisualElement root)
        {
        }
    }
}
