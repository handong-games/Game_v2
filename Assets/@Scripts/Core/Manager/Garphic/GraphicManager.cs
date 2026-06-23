using System;
using Game.Core.Managers.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.Garphic
{
    public partial class GraphicManager : IDisposable
    {
        private GraphicSettingsState _settings;
        private GameObject _managerObject;
        private readonly SaveManager _saveManager;
        private bool _initialized;

        public GraphicManager(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _managerObject = new GameObject("GraphicManager");
            Object.DontDestroyOnLoad(_managerObject);
            _managerObject.AddComponent<GraphicManagerBehaviour>();
            _settings = _saveManager.GetState<GraphicSettingsState>();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _initialized = false;
            if (_managerObject != null)
            {
                Object.Destroy(_managerObject);
                _managerObject = null;
            }

            _settings = null;
        }
    }
}
