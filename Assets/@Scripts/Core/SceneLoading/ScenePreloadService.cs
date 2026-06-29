using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.Core.SceneLoading
{
    public sealed class ScenePreloadService
    {
        private readonly Dictionary<GameSceneId, IScenePreloader> _preloaders = new();

        public ScenePreloadService(IScenePreloader[] preloaders)
        {
            if (preloaders == null)
                throw new ArgumentNullException(nameof(preloaders));

            for (int i = 0; i < preloaders.Length; i++)
            {
                IScenePreloader preloader = preloaders[i];
                if (preloader == null)
                    throw new ArgumentException("Scene preloader list contains null.", nameof(preloaders));

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
