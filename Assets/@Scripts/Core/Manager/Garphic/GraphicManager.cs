using Game.Core.Managers.Dependency;
using Game.Core.Managers.Save;
using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    [ManagerDependency(typeof(SaveManager))]
    public partial class GraphicManager : BaseManager<GraphicManager>
    {
        private GraphicSettingsState _settings;
        private GameObject _managerObject;
        
        protected override void OnInit()
        {
            _managerObject = new GameObject("GraphicManager");
            Object.DontDestroyOnLoad(_managerObject);
            _managerObject.AddComponent<GraphicManagerBehaviour>();
        }

        protected override void OnPostInit()
        {
            _settings = DependencyManager.Instance.Resolve<GraphicSettingsState>();
        }

        protected override void OnDispose()
        {
            if (_managerObject != null)
            {
                Object.Destroy(_managerObject);
                _managerObject = null;
            }

            _settings = null;
        }
    }
}
