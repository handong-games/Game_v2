using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Game.Core.Ports
{
    public interface IViewHost
    {
        VisualElement RootLayer { get; }

        void Attach(BaseView view);
        void Detach(BaseView view);
        void DetachAll();
    }
}
