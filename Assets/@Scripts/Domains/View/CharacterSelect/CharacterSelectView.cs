using Game.Core.Managers.Dependency;
using Game.Core.Managers.View;
using Game.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.CharacterSelect
{
    public class CharacterSelectView : BaseView
    {
        private const int CardRevealStartMs = 120;
        private const int CardRevealStepMs = 80;
        private const int ButtonsRevealMs = 420;
        private const int DetailRevealDelayMs = 70;
        private const int LockedFeedbackDurationMs = 140;
        private const int MaxVisibleSkills = 3;

        private const string DimVisibleClass = "character-select__dim--active";
        private const string TitleVisibleClass = "character-select__header--visible";
        private const string CardsVisibleClass = "character-select__card--visible";
        private const string ButtonsVisibleClass = "character-select__actions--visible";
        private const string DetailVisibleClass = "character-select__detail-panel--visible";
        private const string StartVisibleClass = "character-select__action-button--start--visible";
        private const string ClosingClass = "character-select--closing";
        private const string SelectedClass = "character-select__card--selected";
        private const string SubduedClass = "character-select__card--subdued";
        private const string LockedClass = "character-select__card--locked";
        private const string LockedFeedbackLeftClass = "character-select__card--locked-feedback-left";
        private const string LockedFeedbackRightClass = "character-select__card--locked-feedback-right";

        [Inject]
        private CharacterSelectController _controller;

        private VisualElement _screenRoot;
        private VisualElement _dimBackground;
        private VisualElement _titleArea;
        private VisualElement _cardContainer;
        private VisualElement _detailPanel;
        private VisualElement _skillList;
        private VisualElement _buttons;
        private Button _backButton;
        private Button _startButton;
        private Label _detailName;
        private Label _detailHp;
        private Label _detailCoin;

        private VisualElement[] _cards;
        private VisualElement[] _cardIllustrations;
        private Label[] _cardNames;
        private EventCallback<PointerDownEvent>[] _cardPointerHandlers;
        private SO_CharacterData[] _characters;
        private int _selectedIndex;
        private bool _isClosing;

        protected override void OnBind(VisualElement root)
        {
            if (Root.childCount == 0)
                return;

            _screenRoot = Root.Q<VisualElement>("character-select-root");
            _dimBackground = Root.Q<VisualElement>("dim-bg");
            _titleArea = Root.Q<VisualElement>(className: "character-select__header");
            _cardContainer = Root.Q<VisualElement>("card-container");
            _detailPanel = Root.Q<VisualElement>("detail-panel");
            _detailName = Root.Q<Label>("detail-name");
            _detailHp = Root.Q<Label>("detail-hp");
            _detailCoin = Root.Q<Label>("detail-coin");
            _skillList = Root.Q<VisualElement>("skill-list");
            _buttons = Root.Q<VisualElement>(className: "character-select__actions");
            _backButton = Root.Q<Button>("btn-back");
            _startButton = Root.Q<Button>("btn-start");

            _isClosing = false;
            _selectedIndex = -1;

            if (_screenRoot != null)
            {
                _screenRoot.RemoveFromClassList(ClosingClass);
                _screenRoot.RegisterCallback<TransitionEndEvent>(OnClose);
            }

            CacheCards();
            LoadCharacters();
            ApplyCardData();
            ResetSelectionState();
            ApplyEntranceStates();

            if (_backButton != null)
            {
                _backButton.clicked += OnClickBackButton;
            }

            if (_startButton != null)
            {
                _startButton.clicked += OnClickStartButton;
                _startButton.SetEnabled(false);
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

            if (_startButton != null)
            {
                _startButton.clicked -= OnClickStartButton;
            }

            if (_cards != null && _cardPointerHandlers != null)
            {
                for (int i = 0; i < _cards.Length; i++)
                {
                    if (_cards[i] != null && _cardPointerHandlers[i] != null)
                    {
                        _cards[i].UnregisterCallback(_cardPointerHandlers[i]);
                    }
                }
            }

            _screenRoot = null;
            _dimBackground = null;
            _titleArea = null;
            _cardContainer = null;
            _detailPanel = null;
            _skillList = null;
            _buttons = null;
            _backButton = null;
            _startButton = null;
            _detailName = null;
            _detailHp = null;
            _detailCoin = null;
            _cards = null;
            _cardIllustrations = null;
            _cardNames = null;
            _cardPointerHandlers = null;
            _characters = null;
            _selectedIndex = -1;
            _isClosing = false;
        }

        private void CacheCards()
        {
            _cards = new VisualElement[3];
            _cardIllustrations = new VisualElement[3];
            _cardNames = new Label[3];
            _cardPointerHandlers = new EventCallback<PointerDownEvent>[3];

            for (int i = 0; i < _cards.Length; i++)
            {
                int index = i;
                _cards[index] = Root.Q<VisualElement>($"card-{index}");
                if (_cards[index] == null)
                    continue;

                _cardIllustrations[index] = _cards[index].Q<VisualElement>("card-illustration");
                _cardNames[index] = _cards[index].Q<Label>("card-name");

                EventCallback<PointerDownEvent> handler = _ => OnCardPointerDown(index);
                _cardPointerHandlers[index] = handler;
                _cards[index].RegisterCallback(handler);
            }
        }

        private void LoadCharacters()
        {
            _characters = new SO_CharacterData[_cards.Length];
            if (_controller == null)
                return;

            var characters = _controller.GetAllCharacters();
            int count = Mathf.Min(characters.Count, _characters.Length);
            for (int i = 0; i < count; i++)
            {
                _characters[i] = characters[i];
            }
        }

        private void ApplyCardData()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                    continue;

                SO_CharacterData character = GetCharacterData(i);
                bool hasCharacter = character != null;

                _cards[i].style.display = hasCharacter ? DisplayStyle.Flex : DisplayStyle.None;
                if (!hasCharacter)
                    continue;

                if (_cardNames[i] != null)
                {
                    _cardNames[i].text = character.CharacterName ?? string.Empty;
                }

                _cards[i].EnableInClassList(LockedClass, character.IsLocked);

                if (_cardIllustrations[i] != null && character.Illustration != null)
                {
                    _cardIllustrations[i].style.backgroundImage = new StyleBackground(character.Illustration);
                }
            }
        }

        private void ResetSelectionState()
        {
            _selectedIndex = -1;
            _controller?.ResetSelection();
            _detailPanel?.RemoveFromClassList(DetailVisibleClass);
            _startButton?.RemoveFromClassList(StartVisibleClass);

            if (_startButton != null)
            {
                _startButton.SetEnabled(false);
            }

            if (_detailName != null)
            {
                _detailName.text = string.Empty;
            }

            if (_detailHp != null)
            {
                _detailHp.text = string.Empty;
            }

            if (_detailCoin != null)
            {
                _detailCoin.text = string.Empty;
            }

            _skillList?.Clear();

            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                    continue;

                _cards[i].RemoveFromClassList(SelectedClass);
                _cards[i].RemoveFromClassList(SubduedClass);
                _cards[i].RemoveFromClassList(LockedFeedbackLeftClass);
                _cards[i].RemoveFromClassList(LockedFeedbackRightClass);

                SO_CharacterData character = GetCharacterData(i);
                _cards[i].EnableInClassList(LockedClass, character != null && character.IsLocked);
            }
        }

        private void ApplyEntranceStates()
        {
            _dimBackground?.AddToClassList(DimVisibleClass);
            _titleArea?.AddToClassList(TitleVisibleClass);

            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                    continue;

                VisualElement card = _cards[i];
                int delay = CardRevealStartMs + (i * CardRevealStepMs);
                card.schedule.Execute(() => card.AddToClassList(CardsVisibleClass)).StartingIn(delay);
            }

            _buttons?.schedule.Execute(() => _buttons.AddToClassList(ButtonsVisibleClass)).StartingIn(ButtonsRevealMs);
        }

        private void OnCardPointerDown(int index)
        {
            if (_isClosing)
                return;

            SO_CharacterData character = GetCharacterData(index);
            if (character == null)
                return;

            if (character.IsLocked)
            {
                TriggerLockedFeedback(index);
                return;
            }

            if (_selectedIndex == index)
                return;

            _selectedIndex = index;
            _controller?.SelectCharacter(character);

            _detailPanel?.RemoveFromClassList(DetailVisibleClass);
            _startButton?.RemoveFromClassList(StartVisibleClass);
            if (_startButton != null)
            {
                _startButton.SetEnabled(false);
            }

            RefreshCardState();

            Root.schedule.Execute(() =>
            {
                if (_selectedIndex != index)
                    return;

                BindDetailPanel(character);
                _detailPanel?.AddToClassList(DetailVisibleClass);
                _startButton?.AddToClassList(StartVisibleClass);
                _startButton?.SetEnabled(true);
            }).StartingIn(DetailRevealDelayMs);
        }

        private void RefreshCardState()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                    continue;

                _cards[i].RemoveFromClassList(SelectedClass);
                _cards[i].RemoveFromClassList(SubduedClass);

                if (_selectedIndex < 0)
                    continue;

                SO_CharacterData character = GetCharacterData(i);
                if (character == null || character.IsLocked)
                    continue;

                if (i == _selectedIndex)
                {
                    _cards[i].AddToClassList(SelectedClass);
                }
                else
                {
                    _cards[i].AddToClassList(SubduedClass);
                }
            }
        }

        private void BindDetailPanel(SO_CharacterData character)
        {
            if (character == null)
                return;

            _detailName.text = character.CharacterName ?? string.Empty;
            _detailHp.text = character.InitialMaxHp.ToString();
            _detailCoin.text = character.InitialCoinCount.ToString();

            _skillList.Clear();
            if (character.DefaultSkills != null)
            {
                for (int i = 0; i < character.DefaultSkills.Length && i < MaxVisibleSkills; i++)
                {
                    SO_SkillData skill = character.DefaultSkills[i];
                    if (skill == null)
                        continue;

                    VisualElement tag = new VisualElement();
                    tag.AddToClassList("skill-tag");

                    Label label = new Label(skill.SkillName ?? string.Empty);
                    label.AddToClassList("skill-tag__name");
                    tag.Add(label);

                    VisualElement accent = new VisualElement();
                    accent.AddToClassList("skill-tag__accent");
                    tag.Add(accent);

                    _skillList.Add(tag);
                }
            }

        }

        private void TriggerLockedFeedback(int index)
        {
            if (_cards == null || index < 0 || index >= _cards.Length || _cards[index] == null)
                return;

            string feedbackClass = _selectedIndex >= 0 && index > _selectedIndex
                ? LockedFeedbackRightClass
                : LockedFeedbackLeftClass;

            _cards[index].RemoveFromClassList(LockedFeedbackLeftClass);
            _cards[index].RemoveFromClassList(LockedFeedbackRightClass);
            _cards[index].AddToClassList(feedbackClass);
            _cards[index].schedule.Execute(() => _cards[index].RemoveFromClassList(feedbackClass)).StartingIn(LockedFeedbackDurationMs);
        }

        private void OnClickBackButton()
        {
            if (_isClosing)
                return;

            _isClosing = true;
            _buttons?.SetEnabled(false);
            _screenRoot?.AddToClassList(ClosingClass);
        }

        private void OnClickStartButton()
        {
            if (_selectedIndex < 0)
                return;

            _controller?.StartSelectedCharacter();
        }

        private void OnClose(TransitionEndEvent evt)
        {
            if (!_isClosing || evt.target != _screenRoot)
                return;

            _isClosing = false;
            ViewManager.Instance.Pop();
        }

        private SO_CharacterData GetCharacterData(int index)
        {
            if (_characters == null || index < 0 || index >= _characters.Length)
                return null;

            return _characters[index];
        }
    }
}
