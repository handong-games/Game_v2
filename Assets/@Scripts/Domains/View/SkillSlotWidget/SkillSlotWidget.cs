using Gameplay.GAS;
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
        private bool _hasPendingViewModel;

        public SkillSlotWidget()
        {
            text = string.Empty;
            focusable = false;
            AddToClassList("skill-slot-widget");
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);

            EnsureTemplate();
            _slotTemplate?.CloneTree(this);
        }

        public void Bind(IReadOnlySkillSlotViewModel viewModel)
        {
            _pendingViewModel = viewModel;
            _hasPendingViewModel = true;
            ApplyBinding();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _iconElement = this.Q<VisualElement>("skill-slot-icon");
            _fallbackNameLabel = this.Q<Label>("skill-slot-fallback-name");
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
