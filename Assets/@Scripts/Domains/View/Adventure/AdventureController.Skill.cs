using System.Collections.Generic;
using Domains.View.Widgets;
using Game.AbilitySystem;
using Game.AbilitySystem.Abilities;
using Game.Data;
using Gameplay.GAS;

namespace Domains.Adventure
{
    using Card = global::Domains.Card.Card;

    public sealed partial class AdventureController
    {
        public IReadOnlyList<SkillSlotViewModelV2> GetSkillSlotViewModelsV2()
        {
            Card playerCard = _playerService.GetPlayerCard();
            if (playerCard == null)
                return System.Array.Empty<SkillSlotViewModelV2>();

            GameplayTagContainer skillTags = new();
            skillTags.AddTag(AbilityGameplayTags.Ability_Skill);

            List<GameplayAbilitySpecHandle> handles = new();
            playerCard.AbilitySystem.FindAllAbilitiesWithTags(
                handles,
                skillTags,
                exactMatch: false);

            List<SkillSlotViewModelV2> viewModels = new(handles.Count);
            for (int i = 0; i < handles.Count; i++)
            {
                GameplayAbilitySpecHandle handle = handles[i];
                if (!playerCard.AbilitySystem.TryGetAbilitySpec(handle, out GameplayAbilitySpec spec))
                    continue;

                if (spec.Ability is not SkillGameplayAbility skillAbility)
                    continue;

                viewModels.Add(new SkillSlotViewModelV2(
                    skillAbility.Name,
                    skillAbility.Icon,
                    handle,
                    skillAbility.TargetType));
            }

            return viewModels;
        }

        public IReadOnlyList<SkillSlotViewModel> GetSkillSlotViewModels()
        {
            List<CharacterSkillModel> skillSlots = _playerService.CurrentPlayer.SkillSlots;
            SkillSlotViewModel[] viewModels = new SkillSlotViewModel[skillSlots.Count];

            for (int i = 0; i < skillSlots.Count; i++)
            {
                CharacterSkillModel skill = skillSlots[i];
                viewModels[i] = skill == null
                    ? new SkillSlotViewModel(i, string.Empty, null, SkillType.Attack, true)
                    : new SkillSlotViewModel(i, skill.Name, skill.Icon, skill.SkillType, false);
            }

            return viewModels;
        }

        public SkillSelectionDto TrySelectSkill(int slotIndex)
        {
            CharacterSkillModel skill = GetSkill(slotIndex);
            if (skill == null)
                return new SkillSelectionDto(false, false);

            return new SkillSelectionDto(true, skill.RequiresTarget);
        }

        public bool UseSkill(int slotIndex)
        {
            CharacterSkillModel skill = GetSkill(slotIndex);
            return skill != null && !skill.RequiresTarget;
        }

        public bool UseSkillOnTarget(int slotIndex, uint targetCardId)
        {
            CharacterSkillModel skill = GetSkill(slotIndex);
            if (skill == null || !skill.RequiresTarget)
                return false;

            if (!_cardService.TryGet(targetCardId, out _))
                return false;

            return _combatService.ApplySkill(skill, targetCardId);
        }

        private CharacterSkillModel GetSkill(int slotIndex)
        {
            List<CharacterSkillModel> skillSlots = _playerService.CurrentPlayer.SkillSlots;
            if (slotIndex < 0 || slotIndex >= skillSlots.Count)
                return null;

            return skillSlots[slotIndex];
        }
    }
}
