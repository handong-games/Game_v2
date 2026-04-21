using Domains.Settings;
using Game.Core.Managers.Save;
using UnityEngine;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager : BaseManager<GraphicManager>
    {
        private GraphicSaveData _saveData;
        private GameObject _managerObject;
        
        protected override void OnInit()
        {
            _saveData = SettingManager.Instance.SaveData.GraphicSaveData;

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

            _saveData = null;
        }
    }
}
