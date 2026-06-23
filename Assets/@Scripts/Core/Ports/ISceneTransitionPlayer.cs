using UnityEngine;

namespace Game.Core.Ports
{
    public interface ISceneTransitionPlayer
    {
        Awaitable FadeOut();
    }
}
