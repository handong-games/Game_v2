using Game.Core.Managers.View;

namespace Game.Core.Ports
{
    public interface ISceneViewNavigator
    {
        BaseView Current { get; }

        void Show(BaseView view);
        void Hide(BaseView view);
        void HideCurrent();
        void HideAll();
    }
}
