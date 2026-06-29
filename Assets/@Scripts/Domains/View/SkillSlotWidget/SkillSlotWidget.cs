using Gameplay.GAS;
using System;
using Game.AbilitySystem.Abilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class SkillSlotWidget : Button
    {
        private VisualElement _iconElement;
        private IReadOnlySkillSlotViewModel _pendingViewModel;
        private AdventureSkillSlotViewModel? _adventureSkillSlot;
        private bool _hasPendingViewModel;
        private bool _isAvailable = true;

        internal event Action<SkillSlotWidget, bool> AvailableChanged;

        public SkillSlotWidget()
        {
            throw new InvalidOperationException($"{nameof(SkillSlotWidget)} requires a preloaded VisualTreeAsset template.");
        }

        public SkillSlotWidget(VisualTreeAsset template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            text = string.Empty;
            focusable = false;
            AddToClassList("skill-slot-widget");
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);

            template.CloneTree(this);
            InitializeReferences();
            EnsureReferences();
        }

        public void Bind(IReadOnlySkillSlotViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel.Icon == null)
                throw new InvalidOperationException($"{nameof(SkillSlotWidget)} requires an icon.");

            if (_adventureSkillSlot.HasValue)
            {
                SkillGameplayAbility previousSkillAbility = _adventureSkillSlot.Value.SkillAbility;
                if (previousSkillAbility != null)
                    previousSkillAbility.ActivationStateChanged -= OnActivationStateChanged;
            }

            _adventureSkillSlot = null;

            _pendingViewModel = viewModel;
            _hasPendingViewModel = true;
            ApplyBinding();

            if (viewModel is not AdventureSkillSlotViewModel adventureSkillSlot)
            {
                SetAvailable(true);
                return;
            }

            _adventureSkillSlot = adventureSkillSlot;
            SetAvailable(false);

            if (adventureSkillSlot.SkillAbility != null)
                adventureSkillSlot.SkillAbility.ActivationStateChanged += OnActivationStateChanged;
        }

        public void Unbind()
        {
            if (_adventureSkillSlot.HasValue)
            {
                SkillGameplayAbility skillAbility = _adventureSkillSlot.Value.SkillAbility;
                if (skillAbility != null)
                    skillAbility.ActivationStateChanged -= OnActivationStateChanged;
            }

            _adventureSkillSlot = null;
            _pendingViewModel = null;
            _hasPendingViewModel = false;
            userData = null;
            SetAvailable(true);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            InitializeReferences();
            EnsureReferences();
            ApplyBinding();
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            Unbind();
        }

        private void ApplyBinding()
        {
            if (!_hasPendingViewModel)
                return;

            userData = _pendingViewModel;
            RemoveFromClassList("skill-slot--has-icon");
            AddToClassList("skill-slot--type-attack");

            EnsureReferences();

            _iconElement.style.backgroundImage = StyleKeyword.Null;
            AddToClassList("skill-slot--has-icon");
            _iconElement.style.backgroundImage =
                new StyleBackground(Background.FromSprite(_pendingViewModel.Icon));
        }

        private void OnActivationStateChanged(bool canActivate)
        {
            SetAvailable(canActivate);
        }

        private void SetAvailable(bool canActivate)
        {
            if (_isAvailable == canActivate)
                return;

            _isAvailable = canActivate;
            SetEnabled(canActivate);
            AvailableChanged?.Invoke(this, canActivate);
        }

        private void EnsureReferences()
        {
            if (_iconElement == null)
                throw new InvalidOperationException($"{nameof(SkillSlotWidget)} requires skill-slot-icon.");
        }

        private void InitializeReferences()
        {
            _iconElement ??= this.Q<VisualElement>("skill-slot-icon");
        }

    }
}
