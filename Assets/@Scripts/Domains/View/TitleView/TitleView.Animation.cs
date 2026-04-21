using UnityEngine;
using UnityEngine.UIElements;

namespace Views.TitleView
{
    enum EIntroStep
    {
        None,
        Logo,
        Menu,
        Version,
        Completed
    }
    
    public partial class TitleView
    {
        private const string TitleEchoHiddenClass = "title-screen__branding-echo--hidden";
        private const string TitleTextHiddenClass = "title-screen__branding-text--hidden";
        private const string TitleMenuHiddenClass = "title-screen__menu--hidden";
        private const string VersionHiddenClass = "title-screen__footer-version--hidden";
        
        private EIntroStep _introStep = EIntroStep.None;
        private VisualElement _currentIntroTarget;
        private int _introVersion;
        private bool _introRunning;
        private bool _introCompleted;

        private void StartIntroIfNeeded()
        {
            if (_introCompleted || _introRunning)
            {
                return;
            }

            _introRunning = true;
            int version = ++_introVersion;
            _ = PlayIntroIfNeeded(version);
        }

        private void CancelIntroIfNeeded()
        {
            _introVersion++;
            UnregisterIntroCallbacks();

            if (_introCompleted)
            {
                return;
            }

            _introRunning = false;
            _introStep = EIntroStep.None;
            ResetIntroVisualState();
        }

        private async Awaitable PlayIntroIfNeeded(int version)
        {
            ApplyIntroHiddenState();

            await Awaitable.NextFrameAsync();
            if (!CanContinueIntro(version))
            {
                return;
            }

            _introStep = EIntroStep.Logo;
            _titleEchoText.RemoveFromClassList(TitleEchoHiddenClass);
            _titleText.RemoveFromClassList(TitleTextHiddenClass);

            RegisterIntroTarget(_titleText);
        }

        private void RegisterIntroTarget(VisualElement target)
        {
            UnregisterIntroCallbacks();
            _currentIntroTarget = target;
            _currentIntroTarget.RegisterCallback<TransitionEndEvent>(OnIntroTransitionEnded);
        }

        private void UnregisterIntroCallbacks()
        {
            if (_currentIntroTarget == null)
                return;

            _currentIntroTarget.UnregisterCallback<TransitionEndEvent>(OnIntroTransitionEnded);
            _currentIntroTarget = null;
        }

        private void ResetIntroVisualState()
        {
            UnregisterIntroCallbacks();

            _titleEchoText?.RemoveFromClassList(TitleEchoHiddenClass);
            _titleText?.RemoveFromClassList(TitleTextHiddenClass);
            _titleMenu?.RemoveFromClassList(TitleMenuHiddenClass);
            _versionText?.RemoveFromClassList(VersionHiddenClass);
        }

        private void ApplyIntroHiddenState()
        {
            ResetIntroVisualState();

            _titleEchoText?.AddToClassList(TitleEchoHiddenClass);
            _titleText?.AddToClassList(TitleTextHiddenClass);
            _titleMenu?.AddToClassList(TitleMenuHiddenClass);
            _versionText?.AddToClassList(VersionHiddenClass);
        }

        private void OnIntroTransitionEnded(TransitionEndEvent evt)
        {
            if (!_introRunning || !CanUseIntroElements())
            {
                return;
            }

            if (!HasOpacityTransition(evt))
                return;

            switch (_introStep)
            {
                case EIntroStep.Logo:
                    _introStep = EIntroStep.Menu;
                    RegisterIntroTarget(_titleMenu);
                    _titleMenu.RemoveFromClassList(TitleMenuHiddenClass);
                    break;

                case EIntroStep.Menu:
                    _introStep = EIntroStep.Version;
                    RegisterIntroTarget(_versionText);
                    _versionText.RemoveFromClassList(VersionHiddenClass);
                    break;

                case EIntroStep.Version:
                    _introStep = EIntroStep.Completed;
                    _introRunning = false;
                    _introCompleted = true;
                    UnregisterIntroCallbacks();
                    _newGameButton.Focus();
                    break;
            }
        }

        private bool CanContinueIntro(int version)
        {
            return _introVersion == version &&
                   !_introCompleted &&
                   CanUseIntroElements() &&
                   Root?.panel != null;
        }

        private bool CanUseIntroElements()
        {
            return _titleEchoText != null &&
                   _titleText != null &&
                   _titleMenu != null &&
                   _versionText != null &&
                   _newGameButton != null;
        }

        private static bool HasOpacityTransition(TransitionEndEvent evt)
        {
            foreach (StylePropertyName stylePropertyName in evt.stylePropertyNames)
            {
                if (stylePropertyName.ToString() == "opacity")
                    return true;
            }

            return false;
        }
    }
}
