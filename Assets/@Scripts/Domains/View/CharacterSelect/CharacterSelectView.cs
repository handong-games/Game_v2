using Domains.Character;
using Domains.Scene;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using Game.Data;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.CharacterSelect
{
    public partial class CharacterSelectView : BaseView
    {
        private const int CardRevealStartMs = 120;
        private const int CardRevealStepMs = 80;
        private const int ButtonsRevealMs = 420;
        private const int DetailRevealDelayMs = 70;
        private const int LockedFeedbackDurationMs = 140;
        private const int MaxVisibleSkills = 3;

        private const string TitleVisibleClass = "character-select__header--visible";
        private const string CardsVisibleClass = "character-select__card--visible";
        private const string NavigationVisibleClass = "character-select__navigation--visible";
        private const string DetailVisibleClass = "character-select__detail-panel--visible";
        private const string StartVisibleClass = "character-select__navigation-button--start--visible";
        private const string ClosingClass = "character-select--closing";
        private const string SelectedClass = "character-select__card--selected";
        private const string SubduedClass = "character-select__card--subdued";
        private const string LockedClass = "character-select__card--locked";
        private const string LockedFeedbackLeftClass = "character-select__card--locked-feedback-left";
        private const string LockedFeedbackRightClass = "character-select__card--locked-feedback-right";
        private const string SkillTypeAttackClass = "skill-slot--type-attack";
        private const string SkillTypeDefenseClass = "skill-slot--type-defense";
        private const string SkillTypeUtilityClass = "skill-slot--type-utility";

        [Inject]
        private CharacterSelectController _controller;

        private VisualElement _screenRoot;
        private VisualElement _blockerBackground;
        private VisualElement _header;
        private VisualElement _cardList;
        private VisualElement _detailPanel;
        private VisualElement _skillList;
        private VisualElement _navigation;
        private Button _backButton;
        private Button _startButton;
        private Label _detailName;
        private Label _detailHp;
        private Label _detailCoin;

        private VisualElement[] _cards;
        private VisualElement[] _cardPortraits;
        private VisualElement[] _skillSlots;
        private VisualElement[] _skillIcons;
        private Label[] _skillFallbackNames;
        private EventCallback<PointerDownEvent>[] _cardPointerHandlers;
        private CharacterState[] _characters;
        private CharacterState _localizedNameCharacter;
        private LocalizedString _localizedName;
        private int _selectedIndex;
        private bool _isClosing;
        private CloseReason _closeReason;

        private enum CloseReason
        {
            None,
            Back,
            Start,
        }

        protected override void OnBind(VisualElement root)
        {
            _screenRoot = Root.Q<VisualElement>("character-select-root");
            _blockerBackground = Root.Q<VisualElement>("blocker");
            _header = Root.Q<VisualElement>("header");
            _cardList = Root.Q<VisualElement>("card-list");
            _detailPanel = Root.Q<VisualElement>("detail-panel");
            _detailName = Root.Q<Label>("detail-name");
            _detailHp = Root.Q<Label>("detail-hp");
            _detailCoin = Root.Q<Label>("detail-coin");
            _skillList = Root.Q<VisualElement>("skill-list");
            _navigation = Root.Q<VisualElement>("navigation");
            _backButton = Root.Q<Button>("btn-back");
            _startButton = Root.Q<Button>("btn-start");

            _isClosing = false;
            _closeReason = CloseReason.None;
            _selectedIndex = -1;

            _screenRoot.RemoveFromClassList(ClosingClass);
            _screenRoot.RegisterCallback<TransitionEndEvent>(OnClose);

            CacheCards();
            CacheSkillSlots();
            LoadCharacters();
            ApplyCardData();
            ResetSelectionState();
            _ = PlayIntroAnimation();
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

            UnbindLocalizedName();

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
            _blockerBackground = null;
            _header = null;
            _cardList = null;
            _detailPanel = null;
            _skillList = null;
            _navigation = null;
            _backButton = null;
            _startButton = null;
            _detailName = null;
            _detailHp = null;
            _detailCoin = null;
            _cards = null;
            _cardPortraits = null;
            _skillSlots = null;
            _skillIcons = null;
            _skillFallbackNames = null;
            _cardPointerHandlers = null;
            _characters = null;
            _localizedNameCharacter = null;
            _localizedName = null;
            _selectedIndex = -1;
            _isClosing = false;
            _closeReason = CloseReason.None;
        }

        private void CacheCards()
        {
            _cards = new VisualElement[3];
            _cardPortraits = new VisualElement[3];
            _cardPointerHandlers = new EventCallback<PointerDownEvent>[3];

            for (int i = 0; i < _cards.Length; i++)
            {
                int index = i;
                _cards[index] = Root.Q<VisualElement>($"card-{index}");
                if (_cards[index] == null)
                    continue;

                _cardPortraits[index] = _cards[index].Q<VisualElement>("card-portrait");

                EventCallback<PointerDownEvent> handler = _ => OnCardPointerDown(index);
                _cardPointerHandlers[index] = handler;
                _cards[index].RegisterCallback(handler);
            }
        }

        private void CacheSkillSlots()
        {
            _skillSlots = new VisualElement[MaxVisibleSkills];
            _skillIcons = new VisualElement[MaxVisibleSkills];
            _skillFallbackNames = new Label[MaxVisibleSkills];

            for (int i = 0; i < MaxVisibleSkills; i++)
            {
                _skillSlots[i] = Root.Q<VisualElement>($"skill-{i}");
                if (_skillSlots[i] == null)
                    continue;

                _skillIcons[i] = _skillSlots[i].Q<VisualElement>("skill-slot-icon");
                _skillFallbackNames[i] = _skillSlots[i].Q<Label>("skill-slot-fallback-name");
            }
        }

        private void LoadCharacters()
        {
            _characters = new CharacterState[_cards.Length];
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

                CharacterState character = GetCharacterData(i);
                bool hasCharacter = character != null;

                _cards[i].style.display = hasCharacter ? DisplayStyle.Flex : DisplayStyle.None;
                if (!hasCharacter)
                    continue;

                _cards[i].EnableInClassList(LockedClass, !character.IsUnlocked);

                if (_cardPortraits[i] != null && character.Portrait != null)
                {
                    _cardPortraits[i].style.backgroundImage = new StyleBackground(Background.FromSprite(character.Portrait));
                }
            }
        }

        private void ResetSelectionState()
        {
            _selectedIndex = -1;
            UnbindLocalizedName();
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

            ResetSkillSlots();

            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                    continue;

                _cards[i].RemoveFromClassList(SelectedClass);
                _cards[i].RemoveFromClassList(SubduedClass);
                _cards[i].RemoveFromClassList(LockedFeedbackLeftClass);
                _cards[i].RemoveFromClassList(LockedFeedbackRightClass);

                CharacterState character = GetCharacterData(i);
                _cards[i].EnableInClassList(LockedClass, character != null && !character.IsUnlocked);
            }
        }

        private void ApplyEntranceStates()
        {
            _header?.AddToClassList(TitleVisibleClass);

            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null)
                    continue;

                VisualElement card = _cards[i];
                int delay = CardRevealStartMs + (i * CardRevealStepMs);
                card.schedule.Execute(() => card.AddToClassList(CardsVisibleClass)).StartingIn(delay);
            }

            _navigation?.schedule.Execute(() => _navigation.AddToClassList(NavigationVisibleClass)).StartingIn(ButtonsRevealMs);
        }

        private void OnCardPointerDown(int index)
        {
            if (_isClosing)
                return;

            CharacterState character = GetCharacterData(index);
            if (character == null)
                return;

            if (_controller == null || !_controller.CanSelect(character))
            {
                TriggerLockedFeedback(index);
                return;
            }

            if (_selectedIndex == index)
                return;

            _selectedIndex = index;

            _detailPanel?.RemoveFromClassList(DetailVisibleClass);
            _startButton?.RemoveFromClassList(StartVisibleClass);
            if (_startButton != null)
            {
                _startButton.SetEnabled(false);
            }

            RefreshCardState();

            Root.schedule.Execute(() =>
            {
                if (_isClosing || _selectedIndex != index)
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

                CharacterState character = GetCharacterData(i);
                if (character == null || !character.IsUnlocked)
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

        private void BindDetailPanel(CharacterState character)
        {
            BindLocalizedName(character);
            _detailHp.text = $"HP {character.InitialMaxHp}";
            _detailCoin.text = $"COIN {character.InitialCoinCount}";

            ResetSkillSlots();
            if (character.DefaultSkills != null)
            {
                for (int i = 0; i < character.DefaultSkills.Length && i < MaxVisibleSkills; i++)
                {
                    BindSkillSlot(i, character.DefaultSkills[i]);
                }
            }

        }

        private void BindLocalizedName(CharacterState character)
        {
            UnbindLocalizedName();

            if (character?.LocalizedName == null || character.LocalizedName.IsEmpty)
            {
                SetDetailName(character, null);
                return;
            }

            _localizedNameCharacter = character;
            _localizedName = character.LocalizedName;
            SetDetailName(character, _localizedName.GetLocalizedString());
            _localizedName.StringChanged += OnLocalizedNameChanged;
        }

        private void UnbindLocalizedName()
        {
            if (_localizedName != null)
            {
                _localizedName.StringChanged -= OnLocalizedNameChanged;
            }

            _localizedNameCharacter = null;
            _localizedName = null;
        }

        private void OnLocalizedNameChanged(string value)
        {
            SetDetailName(_localizedNameCharacter, value);
        }

        private void SetDetailName(CharacterState character, string localizedValue)
        {
            if (_detailName == null)
                return;

            _detailName.text = !string.IsNullOrWhiteSpace(localizedValue)
                ? localizedValue
                : character?.Name ?? string.Empty;
        }

        private void ResetSkillSlots()
        {
            for (int i = 0; i < MaxVisibleSkills; i++)
            {
                BindSkillSlot(i, null);
            }
        }

        private void BindSkillSlot(int index, CharacterSkillModel skill)
        {
            if (_skillSlots == null || index < 0 || index >= _skillSlots.Length || _skillSlots[index] == null)
                return;

            VisualElement slot = _skillSlots[index];
            slot.style.display = DisplayStyle.None;
            slot.RemoveFromClassList(SkillTypeAttackClass);
            slot.RemoveFromClassList(SkillTypeDefenseClass);
            slot.RemoveFromClassList(SkillTypeUtilityClass);

            if (_skillIcons[index] != null)
            {
                _skillIcons[index].style.display = DisplayStyle.None;
                _skillIcons[index].style.backgroundImage = StyleKeyword.Null;
            }

            if (_skillFallbackNames[index] != null)
            {
                _skillFallbackNames[index].style.display = DisplayStyle.None;
                _skillFallbackNames[index].text = string.Empty;
            }

            if (skill == null)
                return;

            slot.style.display = DisplayStyle.Flex;
            slot.AddToClassList(GetSkillTypeClass(skill.SkillType));

            if (skill.Icon != null && _skillIcons[index] != null)
            {
                _skillIcons[index].style.display = DisplayStyle.Flex;
                _skillIcons[index].style.backgroundImage = new StyleBackground(Background.FromSprite(skill.Icon));
                return;
            }

            if (_skillFallbackNames[index] != null)
            {
                _skillFallbackNames[index].style.display = DisplayStyle.Flex;
                _skillFallbackNames[index].text = skill.Name ?? string.Empty;
            }
        }

        private static string GetSkillTypeClass(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.Defense => SkillTypeDefenseClass,
                SkillType.Utility => SkillTypeUtilityClass,
                _ => SkillTypeAttackClass,
            };
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
            Close(CloseReason.Back);
        }

        private void OnClickStartButton()
        {
            if (_selectedIndex < 0)
                return;

            CharacterState character = GetCharacterData(_selectedIndex);
            _controller.StartGame(character);
        }

        private void OnClose(TransitionEndEvent evt)
        {
            if (!_isClosing || evt.target != _screenRoot)
                return;

            CloseReason closeReason = _closeReason;
            _isClosing = false;
            _closeReason = CloseReason.None;

            switch (closeReason)
            {
                case CloseReason.Back:
                    ViewManager.Instance.Pop();
                    break;
                case CloseReason.Start:
                    SceneManagerEx.Instance.LoadScene<CombatScene>();
                    break;
            }
        }

        private void Close(CloseReason closeReason)
        {
            _isClosing = true;
            _closeReason = closeReason;
            _navigation?.SetEnabled(false);
            _cardList?.SetEnabled(false);
            _screenRoot?.AddToClassList(ClosingClass);
        }

        private CharacterState GetCharacterData(int index)
        {
            if (_characters == null || index < 0 || index >= _characters.Length)
                return null;

            return _characters[index];
        }
    }
}
