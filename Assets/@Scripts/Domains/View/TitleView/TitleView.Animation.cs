using UnityEngine;
using UnityEngine.UIElements;

namespace Views.TitleView
{
    public partial class TitleView
    {
        private const string TitleIntroClass = "title-screen__intro";
        private const string TitleIntroHiddenClass = "title-screen__intro-hidden";

        private async Awaitable PlayIntroAnimation()
        {
            PrepareIntro(_titleLogo);
            PrepareIntro(_titleMenu);
            PrepareIntro(_titleVersion);

            await Awaitable.NextFrameAsync();

            Root.schedule.Execute(() => ShowIntro(_titleLogo)).StartingIn(0);
            Root.schedule.Execute(() => ShowIntro(_titleMenu)).StartingIn(700);
            Root.schedule.Execute(() => ShowIntro(_titleVersion)).StartingIn(1400);
        }

        private void PrepareIntro(VisualElement element)
        {
            if (element == null)
                return;

            element.AddToClassList(TitleIntroClass);
            element.AddToClassList(TitleIntroHiddenClass);
        }

        private void ShowIntro(VisualElement element)
        {
            if (element == null)
                return;

            element.RemoveFromClassList(TitleIntroHiddenClass);
        }
    }
}
