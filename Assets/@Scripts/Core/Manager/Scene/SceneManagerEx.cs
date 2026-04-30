using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Managers.Scene
{
    public class SceneManagerEx : BaseManager<SceneManagerEx>
    {
        private bool _isLoading;
        private BaseScene _currentScene;
        private BaseScene _prevScene;

        protected override void OnInit()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _currentScene = null;
            _prevScene = null;
        }
        
        // 첫번째 씬 로드
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnFirstSceneLoaded()
        {
            Type sceneType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith("Assembly-CSharp", StringComparison.Ordinal))
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == SceneManager.GetActiveScene().name && typeof(BaseScene).IsAssignableFrom(t));
            
            Instance._currentScene = (BaseScene)Activator.CreateInstance(sceneType);
            Instance._currentScene.Loaded();
        }
        
        // 새로운 씬로드
        public async void LoadScene<T>() where T : BaseScene, new()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            await _currentScene.BeforeUnload();
            _prevScene = _currentScene;
            
            // 새로운 씬 저장 및 로드
            _currentScene = new T();
            
            // 새로운 씬 이동
            SceneManager.LoadScene(typeof(T).Name);
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (scene.name != _currentScene.GetType().Name)
                return;

            _currentScene.Loaded();
            _isLoading = false;
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            if (_prevScene == null)
                return;

            if (scene.name != _prevScene.GetType().Name)
                return;

            _prevScene.Unloaded();
            _prevScene = null;
        }
    }
}
