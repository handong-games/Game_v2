using System.Collections.Generic;
using Game.Data;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class SkillSlotWidget : VisualElement
    {
        private const string SkillSlotAddress = "SkillSlot";
        private const string SlotClass = "skill-slot-widget__slot";
        private const string HiddenClass = "ui-transition--hidden";
        private const string FromBottomClass = "ui-transition--from-bottom";
        private const string EnterClass = "ui-transition--enter";
        private const string SlotEmptyClass = "skill-slot--empty";
        private const string SkillTypeAttackClass = "skill-slot--type-attack";
        private const string SkillTypeDefenseClass = "skill-slot--type-defense";
        private const string SkillTypeUtilityClass = "skill-slot--type-utility";

        private readonly List<VisualElement> _slots = new();
        private readonly List<VisualElement> _icons = new();
        private readonly List<Label> _fallbackNames = new();

        private VisualTreeAsset _slotTemplate;
        private bool _isShown;

        public void Bind(IReadOnlyList<CharacterSkillModel> skills)
        {
            int count = skills?.Count ?? 0;
            EnsureSlotCount(count);

            for (int i = 0; i < _slots.Count; i++)
            {
                bool visible = i < count;
                CharacterSkillModel skill = visible ? skills[i] : null;
                BindSlot(i, skill, visible);
            }
        }

        public async Awaitable Show()
        {
            if (_isShown)
                return;

            _isShown = true;
            SetHidden();

            await ViewTransitionManager.Instance.Play(this, EnterClass);
        }

        public void Hide()
        {
            _isShown = false;
            SetHidden();
        }

        private void EnsureSlotCount(int count)
        {
            EnsureTemplate();
            if (_slotTemplate == null)
                return;

            while (_slots.Count < count)
            {
                AddSlot();
            }
        }

        private void AddSlot()
        {
            TemplateContainer slot = _slotTemplate.CloneTree();
            slot.AddToClassList(SlotClass);
            Add(slot);

            _slots.Add(slot);
            _icons.Add(slot.Q<VisualElement>("skill-slot-icon"));
            _fallbackNames.Add(slot.Q<Label>("skill-slot-fallback-name"));
        }

        private void BindSlot(int index, CharacterSkillModel skill, bool visible)
        {
            VisualElement slot = _slots[index];
            slot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            slot.RemoveFromClassList(SlotEmptyClass);
            slot.RemoveFromClassList(SkillTypeAttackClass);
            slot.RemoveFromClassList(SkillTypeDefenseClass);
            slot.RemoveFromClassList(SkillTypeUtilityClass);

            VisualElement icon = _icons[index];
            if (icon != null)
            {
                icon.style.display = DisplayStyle.None;
                icon.style.backgroundImage = StyleKeyword.Null;
            }

            Label fallbackName = _fallbackNames[index];
            if (fallbackName != null)
            {
                fallbackName.style.display = DisplayStyle.None;
                fallbackName.text = string.Empty;
            }

            if (!visible)
                return;

            if (skill == null)
            {
                slot.AddToClassList(SlotEmptyClass);
                return;
            }

            slot.AddToClassList(GetSkillTypeClass(skill.SkillType));

            if (skill.Icon != null && icon != null)
            {
                icon.style.display = DisplayStyle.Flex;
                icon.style.backgroundImage = new StyleBackground(Background.FromSprite(skill.Icon));
                return;
            }

            if (fallbackName != null)
            {
                fallbackName.style.display = DisplayStyle.Flex;
                fallbackName.text = skill.Name ?? string.Empty;
            }
        }

        private void EnsureTemplate()
        {
            if (_slotTemplate != null)
                return;

            _slotTemplate = Addressables
                .LoadAssetAsync<VisualTreeAsset>(SkillSlotAddress)
                .WaitForCompletion();

            if (_slotTemplate == null)
            {
                Debug.LogError($"{nameof(SkillSlotWidget)} failed to load {SkillSlotAddress}.");
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

        private void SetHidden()
        {
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }
    }
}
