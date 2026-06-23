using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.SceneLoading
{
    public sealed class ScenePreloadService
    {
        private readonly Dictionary<GameSceneId, IScenePreloader> _preloaders = new();

        public ScenePreloadService(IScenePreloader[] preloaders)
        {
            for (int i = 0; i < preloaders.Length; i++)
            {
                IScenePreloader preloader = preloaders[i];
                _preloaders.Add(preloader.SceneId, preloader);
            }
        }

        public Awaitable Preload(GameSceneId sceneId)
        {
            if (_preloaders.TryGetValue(sceneId, out IScenePreloader preloader))
                return preloader.Preload();

            return Awaitable.NextFrameAsync();
        }
    }
}
