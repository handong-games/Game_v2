using UnityEngine;

namespace Views.TitleView
{
    public partial class TitleView
    {
        private const string TitleIntroShownClass = "title-screen--intro-shown";

        private async Awaitable PlayIntroAnimation()
        {
            Root.RemoveFromClassList(TitleIntroShownClass);

            await Awaitable.NextFrameAsync();

            Root.AddToClassList(TitleIntroShownClass);
        }
    }
}
