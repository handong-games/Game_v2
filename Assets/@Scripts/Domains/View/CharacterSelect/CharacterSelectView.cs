using System;
using Domains.Adventure;
using Domains.Card;
using Domains.View.Widgets;
using Game.Core.Managers.View;
using Game.Data;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.CharacterSelect
{
    public sealed partial class CharacterSelectView : BaseView
    {
        private const int CardCount = 3;

        private const string ShownClass = "character-select--shown";
        private const string DetailVisibleClass = "character-select__detail-panel--visible";
        private const string StartVisibleClass = "character-select__navigation-button--start--visible";
        private const string ClosingClass = "character-select--closing";
        private const string EmptyCardClass = "character-select__card--empty";
        private const string SelectedClass = "character-select__card--selected";
        private const string SubduedClass = "character-select__card--subdued";
        private const string LockedFeedbackLeftClass = "character-select__card--locked-feedback-left";
        private const string LockedFeedbackRightClass = "character-select__card--locked-feedback-right";

        private readonly CharacterSelectController _controller;

        private VisualElement _screenRoot;
        private VisualElement _cardList;
        private VisualElement _detailPanel;
        private VisualElement _navigation;
        private Button _backButton;
        private Button _startButton;
        private Label _detailName;
        private Label _detailHp;
        private Label _detailCoin;
        private CharacterSelectSkillSlotGroup _skillSlotGroup;

        private VisualElement[] _cards;
        private VisualElement[] _cardFeedbackRoots;
        private CardWidget[] _cardWidgets;
        private EventCallback<PointerDownEvent>[] _cardPointerHandlers;
        private CharacterSelectCardViewModel[] _cardViewModels;
        private LocalizedString _localizedName;
        private int _selectedIndex;
        private bool _isClosing;

        public CharacterSelectView(CharacterSelectController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        protected override void OnVisualTreeCloned(VisualElement root)
        {
            _screenRoot = Root.Q<VisualElement>("character-select-root");
            _cardList = Root.Q<VisualElement>("card-list");
            _detailPanel = Root.Q<VisualElement>("detail-panel");
            _detailName = Root.Q<Label>("detail-name");
            _detailHp = Root.Q<Label>("detail-hp");
            _detailCoin = Root.Q<Label>("detail-coin");
            _skillSlotGroup = Root.Q<CharacterSelectSkillSlotGroup>("skill-slot-group");
            _navigation = Root.Q<VisualElement>("navigation");
            _backButton = Root.Q<Button>("btn-back");
            _startButton = Root.Q<Button>("btn-start");

            _screenRoot.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);

            CacheCards();

            _backButton.clicked += OnClickBackButton;
            _startButton.clicked += OnClickStartButton;
        }

        protected override void OnShown()
        {
            ResetClosingState();
            LoadCharacterCards();
            ApplyCardData();
            ResetSelectionState();
            ResetEntranceStates();
            _ = PlayIntroAnimation();
        }

        protected override void OnHidden()
        {
            ResetEntranceStates();
            UnbindLocalizedName();
        }

        public override void Dispose()
        {
            if (_screenRoot != null)
            {
                _screenRoot.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
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

                    if (_cardFeedbackRoots[i] != null)
                    {
                        _cardFeedbackRoots[i].UnregisterCallback<TransitionEndEvent>(OnCardFeedbackTransitionEnd);
                    }
                }
            }

            _screenRoot = null;
            _cardList = null;
            _detailPanel = null;
            _navigation = null;
            _backButton = null;
            _startButton = null;
            _detailName = null;
            _detailHp = null;
            _detailCoin = null;
            _skillSlotGroup = null;
            _cards = null;
            _cardFeedbackRoots = null;
            _cardWidgets = null;
            _cardPointerHandlers = null;
            _cardViewModels = null;
            _localizedName = null;
            _selectedIndex = -1;
            _isClosing = false;

            base.Dispose();
        }

        private void CacheCards()
        {
            _cards = new VisualElement[CardCount];
            _cardFeedbackRoots = new VisualElement[CardCount];
            _cardWidgets = new CardWidget[CardCount];
            _cardPointerHandlers = new EventCallback<PointerDownEvent>[CardCount];

            for (int i = 0; i < _cards.Length; i++)
            {
                int index = i;
                _cards[index] = Root.Q<VisualElement>($"card-{index}");
                _cardFeedbackRoots[index] = Root.Q<VisualElement>($"card-feedback-{index}");
                _cardWidgets[index] = _cardFeedbackRoots[index].Q<CardWidget>("card");

                EventCallback<PointerDownEvent> handler = _ => OnCardPointerDown(index);
                _cardPointerHandlers[index] = handler;
                _cards[index].RegisterCallback(handler);
                _cardFeedbackRoots[index].RegisterCallback<TransitionEndEvent>(OnCardFeedbackTransitionEnd);
            }
        }

        private void LoadCharacterCards()
        {
            _cardViewModels = new CharacterSelectCardViewModel[_cards.Length];
            CharacterSelectInitialViewModel initialViewModel = _controller.CreateInitialViewModel();
            var cards = initialViewModel.Cards;
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
            _skillSlotGroup?.Bind(System.Array.Empty<CharacterSelectSkillSlotViewModel>());

            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].RemoveFromClassList(SelectedClass);
                _cards[i].RemoveFromClassList(SubduedClass);
                _cardFeedbackRoots[i].RemoveFromClassList(LockedFeedbackLeftClass);
                _cardFeedbackRoots[i].RemoveFromClassList(LockedFeedbackRightClass);

                CharacterSelectCardViewModel card = GetCardData(i);
                if (card != null)
                {
                    BindCharacterCard(i, card);
                }
            }
        }

        private void ResetClosingState()
        {
            _isClosing = false;
            _screenRoot.RemoveFromClassList(ClosingClass);
            _navigation.SetEnabled(true);
            _cardList.SetEnabled(true);
        }

        private void ResetEntranceStates()
        {
            _screenRoot?.RemoveFromClassList(ShownClass);
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

            BindDetailPanel(card);
            _detailPanel.AddToClassList(DetailVisibleClass);
            _startButton.AddToClassList(StartVisibleClass);
            _startButton.SetEnabled(true);
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
            _detailHp.text = $"HP {Mathf.RoundToInt(card.MaxHealth)}";
            _detailCoin.text = $"COIN {card.CoinCount}";
            _skillSlotGroup?.Bind(card.Skills);
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

        private void TriggerLockedFeedback(int index)
        {
            string feedbackClass = _selectedIndex >= 0 && index > _selectedIndex
                ? LockedFeedbackRightClass
                : LockedFeedbackLeftClass;

            VisualElement feedbackRoot = _cardFeedbackRoots[index];
            feedbackRoot.RemoveFromClassList(LockedFeedbackLeftClass);
            feedbackRoot.RemoveFromClassList(LockedFeedbackRightClass);
            feedbackRoot.AddToClassList(feedbackClass);
        }

        private void OnCardFeedbackTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target is not VisualElement feedbackRoot)
                return;

            if (!feedbackRoot.ClassListContains(LockedFeedbackLeftClass) &&
                !feedbackRoot.ClassListContains(LockedFeedbackRightClass))
                return;

            feedbackRoot.RemoveFromClassList(LockedFeedbackLeftClass);
            feedbackRoot.RemoveFromClassList(LockedFeedbackRightClass);
        }

        private void OnClickBackButton()
        {
            if (_isClosing)
                return;

            PlayCloseAnimation();
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

        private void OnCloseAnimationCompleted()
        {
            _isClosing = false;
            _controller.OnBackClosed();
        }

        private CharacterSelectCardViewModel GetCardData(int index)
        {
            if (_cardViewModels == null || index < 0 || index >= _cardViewModels.Length)
                return null;

            return _cardViewModels[index];
        }
    }
}
