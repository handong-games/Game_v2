using Domains.Settings;
using Game.Core.Managers.Save;
using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager : BaseManager<GraphicManager>
    {
        private SettingsState _settings;
        private GameObject _managerObject;
        
        protected override void OnInit()
        {
            _settings = SaveManager.Instance.Settings;

            _managerObject = new GameObject("GraphicManager");
            Object.DontDestroyOnLoad(_managerObject);
            _managerObject.AddComponent<GraphicManagerBehaviour>();
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
