using UnityEngine;

namespace Game.Core.SceneLoading
{
    public interface IScenePreloader
    {
        GameSceneId SceneId { get; }
        Awaitable Preload();
    }
}
