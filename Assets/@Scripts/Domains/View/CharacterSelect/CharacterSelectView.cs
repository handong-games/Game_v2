using Domains.Card;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.Scene;
using Domains.View.Widgets;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.View;
using Game.Data;
using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.CharacterSelect
{
    public sealed class CharacterSelectView : BaseView
    {
        private const int CardRevealStartMs = 120;
        private const int CardRevealStepMs = 80;
        private const int ButtonsRevealMs = 420;
        private const int DetailRevealDelayMs = 70;
        private const int LockedFeedbackDurationMs = 140;
        private const int CardCount = 3;
        private const int MaxVisibleSkills = 3;

        private const string BlockerActiveClass = "character-select__blocker--active";
        private const string TitleVisibleClass = "character-select__header--visible";
        private const string CardsVisibleClass = "character-select__card--visible";
        private const string NavigationVisibleClass = "character-select__navigation--visible";
        private const string DetailVisibleClass = "character-select__detail-panel--visible";
        private const string StartVisibleClass = "character-select__navigation-button--start--visible";
        private const string ClosingClass = "character-select--closing";
        private const string EmptyCardClass = "character-select__card--empty";
        private const string SelectedClass = "character-select__card--selected";
        private const string SubduedClass = "character-select__card--subdued";
        private const string LockedFeedbackLeftClass = "character-select__card--locked-feedback-left";
        private const string LockedFeedbackRightClass = "character-select__card--locked-feedback-right";
        private const string SkillSlotVisibleClass = "character-select__skill-slot-template--visible";
        private const string SkillHasIconClass = "skill-slot--has-icon";
        private const string SkillHasLabelClass = "skill-slot--has-label";
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
        private VisualElement _navigation;
        private Button _backButton;
        private Button _startButton;
        private Label _detailName;
        private Label _detailHp;
        private Label _detailCoin;

        private VisualElement[] _cards;
        private CardWidget[] _cardWidgets;
        private VisualElement[] _skillSlots;
        private VisualElement[] _skillIcons;
        private Label[] _skillFallbackNames;
        private EventCallback<PointerDownEvent>[] _cardPointerHandlers;
        private CharacterSelectCardViewModel[] _cardViewModels;
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
            LoadCharacterCards();
            ApplyCardData();
            ResetSelectionState();
            _ = PlayIntroAnimation();
            ApplyEntranceStates();

            _backButton.clicked += OnClickBackButton;
            _startButton.clicked += OnClickStartButton;
            _startButton.SetEnabled(false);
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
            _navigation = null;
            _backButton = null;
            _startButton = null;
            _detailName = null;
            _detailHp = null;
            _detailCoin = null;
            _cards = null;
            _cardWidgets = null;
            _skillSlots = null;
            _skillIcons = null;
            _skillFallbackNames = null;
            _cardPointerHandlers = null;
            _cardViewModels = null;
            _localizedName = null;
            _selectedIndex = -1;
            _isClosing = false;
            _closeReason = CloseReason.None;
        }

        private void CacheCards()
        {
            _cards = new VisualElement[CardCount];
            _cardWidgets = new CardWidget[CardCount];
            _cardPointerHandlers = new EventCallback<PointerDownEvent>[CardCount];

            for (int i = 0; i < _cards.Length; i++)
            {
                int index = i;
                _cards[index] = Root.Q<VisualElement>($"card-{index}");
                _cardWidgets[index] = _cards[index].Q<CardWidget>("card");

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
                _skillIcons[i] = _skillSlots[i].Q<VisualElement>("skill-slot-icon");
                _skillFallbackNames[i] = _skillSlots[i].Q<Label>("skill-slot-fallback-name");
            }
        }

        private void LoadCharacterCards()
        {
            _cardViewModels = new CharacterSelectCardViewModel[_cards.Length];
            CharacterSelectInitialViewModel initialViewModel = _controller.CreateInitialViewModel();
            IReadOnlyList<CharacterSelectCardViewModel> cards = initialViewModel.Cards;
            int count = Mathf.Min(cards.Count, _cardViewModels.Length);
            for (int i = 0; i < count; i++)
            {
                _cardViewModels[i] = cards[i];
            }
        }

        private void ApplyCardData()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                CharacterSelectCardViewModel card = GetCardData(i);
                bool hasCard = card != null;

                _cards[i].EnableInClassList(EmptyCardClass, !hasCard);
                if (!hasCard)
                    continue;

                BindCharacterCard(i, card);
            }
        }

        private void ResetSelectionState()
        {
            _selectedIndex = -1;
            UnbindLocalizedName();
            _detailPanel.RemoveFromClassList(DetailVisibleClass);
            _startButton.RemoveFromClassList(StartVisibleClass);
            _startButton.SetEnabled(false);
            _detailName.text = string.Empty;
            _detailHp.text = string.Empty;
            _detailCoin.text = string.Empty;

            ResetSkillSlots();

            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].RemoveFromClassList(SelectedClass);
                _cards[i].RemoveFromClassList(SubduedClass);
                _cards[i].RemoveFromClassList(LockedFeedbackLeftClass);
                _cards[i].RemoveFromClassList(LockedFeedbackRightClass);

                CharacterSelectCardViewModel card = GetCardData(i);
                if (card != null)
                {
                    BindCharacterCard(i, card);
                }
            }
        }

        private void BindCharacterCard(int index, CharacterSelectCardViewModel card)
        {
            ECardFace face = card.IsLocked ? ECardFace.Back : ECardFace.Front;
            CardFaceViewModel front = card.IsLocked ? null : card.Face;
            CardFaceViewModel back = card.IsLocked ? card.Face : null;

            _cardWidgets[index].Bind(new CardViewModel(
                face,
                front,
                back));
        }

        private void ApplyEntranceStates()
        {
            _header.AddToClassList(TitleVisibleClass);

            for (int i = 0; i < _cards.Length; i++)
            {
                VisualElement card = _cards[i];
                int delay = CardRevealStartMs + (i * CardRevealStepMs);
                card.schedule.Execute(() => card.AddToClassList(CardsVisibleClass)).StartingIn(delay);
            }

            _navigation.schedule.Execute(() => _navigation.AddToClassList(NavigationVisibleClass)).StartingIn(ButtonsRevealMs);
        }

        private void OnCardPointerDown(int index)
        {
            if (_isClosing)
                return;

            CharacterSelectCardViewModel card = GetCardData(index);
            if (card == null)
                return;

            if (card.IsLocked)
            {
                TriggerLockedFeedback(index);
                return;
            }

            if (_selectedIndex == index)
                return;

            _selectedIndex = index;

            _detailPanel.RemoveFromClassList(DetailVisibleClass);
            _startButton.RemoveFromClassList(StartVisibleClass);
            _startButton.SetEnabled(false);

            RefreshCardSelectionState();

            Root.schedule.Execute(() =>
            {
                if (_isClosing || _selectedIndex != index)
                    return;

                BindDetailPanel(card);
                _detailPanel.AddToClassList(DetailVisibleClass);
                _startButton.AddToClassList(StartVisibleClass);
                _startButton.SetEnabled(true);
            }).StartingIn(DetailRevealDelayMs);
        }

        private void RefreshCardSelectionState()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].RemoveFromClassList(SelectedClass);
                _cards[i].RemoveFromClassList(SubduedClass);

                if (_selectedIndex < 0)
                    continue;

                CharacterSelectCardViewModel card = GetCardData(i);
                if (card == null || card.IsLocked)
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

        private void BindDetailPanel(CharacterSelectCardViewModel card)
        {
            BindLocalizedName(card);
            _detailHp.text = $"HP {card.MaxHp}";
            _detailCoin.text = $"COIN {card.CoinCount}";

            ResetSkillSlots();
            if (card.Skills != null)
            {
                for (int i = 0; i < card.Skills.Count && i < MaxVisibleSkills; i++)
                {
                    BindSkillSlot(i, card.Skills[i]);
                }
            }
        }

        private void BindLocalizedName(CharacterSelectCardViewModel card)
        {
            UnbindLocalizedName();

            if (card?.LocalizedName == null || card.LocalizedName.IsEmpty)
            {
                SetDetailName(null);
                return;
            }

            _localizedName = card.LocalizedName;
            SetDetailName(_localizedName.GetLocalizedString());
            _localizedName.StringChanged += OnLocalizedNameChanged;
        }

        private void UnbindLocalizedName()
        {
            if (_localizedName != null)
            {
                _localizedName.StringChanged -= OnLocalizedNameChanged;
            }

            _localizedName = null;
        }

        private void OnLocalizedNameChanged(string value)
        {
            SetDetailName(value);
        }

        private void SetDetailName(string localizedValue)
        {
            _detailName.text = localizedValue ?? string.Empty;
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
            VisualElement slot = _skillSlots[index];
            slot.RemoveFromClassList(SkillSlotVisibleClass);
            slot.RemoveFromClassList(SkillHasIconClass);
            slot.RemoveFromClassList(SkillHasLabelClass);
            slot.RemoveFromClassList(SkillTypeAttackClass);
            slot.RemoveFromClassList(SkillTypeDefenseClass);
            slot.RemoveFromClassList(SkillTypeUtilityClass);

            _skillIcons[index].style.backgroundImage = StyleKeyword.Null;
            _skillFallbackNames[index].text = string.Empty;

            if (skill == null)
                return;

            slot.AddToClassList(SkillSlotVisibleClass);
            slot.AddToClassList(GetSkillTypeClass(skill.SkillType));

            if (skill.Icon != null)
            {
                slot.AddToClassList(SkillHasIconClass);
                _skillIcons[index].style.backgroundImage = new StyleBackground(Background.FromSprite(skill.Icon));
                return;
            }

            slot.AddToClassList(SkillHasLabelClass);
            _skillFallbackNames[index].text = skill.Name ?? string.Empty;
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

            CharacterSelectCardViewModel card = GetCardData(_selectedIndex);
            if (card == null || card.IsLocked)
                return;

            _controller.StartNewAdventure(card.CharacterId);
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
                    SceneManagerEx.Instance.LoadScene<AdventureScene>();
                    break;
            }
        }

        private void Close(CloseReason closeReason)
        {
            _isClosing = true;
            _closeReason = closeReason;
            _navigation.SetEnabled(false);
            _cardList.SetEnabled(false);
            _screenRoot.AddToClassList(ClosingClass);
        }

        private async Awaitable PlayIntroAnimation()
        {
            await Awaitable.NextFrameAsync();
            _blockerBackground.AddToClassList(BlockerActiveClass);
        }

        private CharacterSelectCardViewModel GetCardData(int index)
        {
            if (_cardViewModels == null || index < 0 || index >= _cardViewModels.Length)
                return null;

            return _cardViewModels[index];
        }
    }
}
