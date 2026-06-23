using System;

namespace Game.Core.SceneLoading
{
    public static class GameSceneNames
    {
        public static string ToSceneName(GameSceneId sceneId)
        {
            return sceneId switch
            {
                GameSceneId.Title => "TitleScene",
                GameSceneId.Adventure => "AdventureScene",
                _ => throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, null)
            };
        }
    }
}
