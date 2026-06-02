using Gameplay.GAS;
using System;
using Game.AbilitySystem.Abilities;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class SkillSlotWidget : Button
    {
        private const string SkillSlotAddress = "SkillSlotWidget";

        private static VisualTreeAsset _slotTemplate;

        private VisualElement _iconElement;
        private Label _fallbackNameLabel;
        private IReadOnlySkillSlotViewModel _pendingViewModel;
        private AdventureSkillSlotViewModel? _adventureSkillSlot;
        private bool _hasPendingViewModel;
        private bool _isAvailable = true;

        public event Action<SkillSlotWidget, bool> AvailableChanged;

        public SkillSlotWidget()
        {
            text = string.Empty;
            focusable = false;
            AddToClassList("skill-slot-widget");
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);

            EnsureTemplate();
            _slotTemplate?.CloneTree(this);
        }

        public void Bind(IReadOnlySkillSlotViewModel viewModel)
        {
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
            _iconElement = this.Q<VisualElement>("skill-slot-icon");
            _fallbackNameLabel = this.Q<Label>("skill-slot-fallback-name");
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
            RemoveFromClassList("skill-slot--has-label");
            AddToClassList("skill-slot--type-attack");

            if (_iconElement != null)
                _iconElement.style.backgroundImage = StyleKeyword.Null;

            if (_fallbackNameLabel != null)
                _fallbackNameLabel.text = string.Empty;

            if (_pendingViewModel?.Icon != null && _iconElement != null)
            {
                AddToClassList("skill-slot--has-icon");
                _iconElement.style.backgroundImage =
                    new StyleBackground(Background.FromSprite(_pendingViewModel.Icon));
                return;
            }

            if (_fallbackNameLabel != null)
            {
                AddToClassList("skill-slot--has-label");
                _fallbackNameLabel.text = _pendingViewModel?.Name?.GetLocalizedString() ?? string.Empty;
            }
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

        private static void EnsureTemplate()
        {
            if (_slotTemplate != null)
                return;

            _slotTemplate = Addressables
                .LoadAssetAsync<VisualTreeAsset>(SkillSlotAddress)
                .WaitForCompletion();

            if (_slotTemplate == null)
                Debug.LogError($"{nameof(SkillSlotWidget)} failed to load {SkillSlotAddress}.");
        }
    }
}
