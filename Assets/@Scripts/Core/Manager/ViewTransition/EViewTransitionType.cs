namespace Game.Core.Managers.View
{
    public enum EViewTransitionType
    {
        /// <summary>
        /// Makes the target VisualElement gradually appear.
        /// </summary>
        FadeIn = 0,

        /// <summary>
        /// Makes the target VisualElement gradually disappear.
        /// </summary>
        FadeOut = 1,

        Count,
    }
}
