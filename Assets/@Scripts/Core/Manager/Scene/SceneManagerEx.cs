using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core.Managers.Scene
{
    public class SceneManagerEx : BaseManager<SceneManagerEx>
    {
        private bool _isLoading;
        private BaseScene _currentBaseScene;

        protected override void OnInit() {}

        protected override void OnDispose()
        {
            _currentBaseScene = null;
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
            
            Instance._currentBaseScene = (BaseScene)Activator.CreateInstance(sceneType);
        }
        
        // 새로운 씬로드
        public async void LoadScene<T>() where T : BaseScene, new()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            await _currentBaseScene.BeforeUnload();
            
            // 새로운 씬 저장 및 로드
            _currentBaseScene = new T();
            
            // 새로운 씬 이동
            SceneManager.LoadScene(typeof(T).Name);

            _isLoading = false;
        }
    }
}
