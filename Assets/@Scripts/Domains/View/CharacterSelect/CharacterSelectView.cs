using Game.Core.Managers.View;
using UnityEngine.UIElements;

namespace Domains.CharacterSelect
{
    public class CharacterSelectView : BaseView
    {
        private const string DimVisibleClass = "character-select__dim--active";
        private const string TitleVisibleClass = "character-select__header--visible";
        private const string CardsVisibleClass = "character-select__card--visible";
        private const string ButtonsVisibleClass = "character-select__actions--visible";
        private const string ClosingClass = "character-select--closing";

        private VisualElement _screenRoot;
        private VisualElement _dimBackground;
        private VisualElement _titleArea;
        private VisualElement _buttons;
        private VisualElement[] _cards;
        private Button _backButton;
        private bool _isClosing;

        protected override void OnBind(VisualElement root)
        {
            if (Root.childCount == 0)
                return;

            _screenRoot = Root.Q<VisualElement>(className: "character-select");
            _dimBackground = Root.Q<VisualElement>("dim-bg");
            _titleArea = Root.Q<VisualElement>(className: "character-select__header");
            _buttons = Root.Q<VisualElement>(className: "character-select__actions");
            _backButton = Root.Q<Button>("btn-back");

            _isClosing = false;
            if (_screenRoot != null)
            {
                _screenRoot.RemoveFromClassList(ClosingClass);
                _screenRoot.RegisterCallback<TransitionEndEvent>(OnClose);
            }

            _cards = new[]
            {
                Root.Q<VisualElement>("card-0"),
                Root.Q<VisualElement>("card-1"),
                Root.Q<VisualElement>("card-2")
            };

            if (_dimBackground != null)
            {
                _dimBackground.AddToClassList(DimVisibleClass);
            }

            if (_titleArea != null)
            {
                _titleArea.AddToClassList(TitleVisibleClass);
            }

            if (_buttons != null)
            {
                _buttons.AddToClassList(ButtonsVisibleClass);
            }

            if (_cards != null)
            {
                for (int i = 0; i < _cards.Length; i++)
                {
                    if (_cards[i] != null)
                    {
                        _cards[i].AddToClassList(CardsVisibleClass);
                    }
                }
            }

            if (_backButton != null)
            {
                _backButton.clicked += OnClickBackButton;
            }
        }

        public override void Dispose()
        {
            if (_screenRoot != null)
            {
                _screenRoot.UnregisterCallback<TransitionEndEvent>(OnClose);
            }

            if (_backButton != null)
            {
                _backButton.clicked -= OnClickBackButton;
            }

            _screenRoot = null;
            _dimBackground = null;
            _titleArea = null;
            _buttons = null;
            _cards = null;
            _backButton = null;
            _isClosing = false;
        }

        private void OnClickBackButton()
        {
            if (_isClosing)
                return;

            _isClosing = true;
            
            _buttons.SetEnabled(false);
            _screenRoot.AddToClassList(ClosingClass);
        }

        private void OnClose(TransitionEndEvent evt)
        {
            if (!_isClosing)
                return;
            
            _isClosing = false;
            ViewManager.Instance.Pop();
        }
    }
}
