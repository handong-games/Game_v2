using System.Collections.Generic;
using Domains.View.Widgets;
using Game.AbilitySystem;
using Game.AbilitySystem.Abilities;
using Gameplay.GAS;

namespace Domains.Adventure
{
    using Card = global::Domains.Card.Card;

    public sealed partial class AdventureController
    {
        public IReadOnlyList<AdventureSkillSlotViewModel> GetSkillSlotViewModels()
        {
            Card playerCard = _playerService.GetPlayerCard();
            if (playerCard == null)
                return System.Array.Empty<AdventureSkillSlotViewModel>();

            GameplayTagContainer skillTags = new();
            skillTags.AddTag(AbilityGameplayTags.AbilitySkill);

            List<GameplayAbilitySpecHandle> handles = new();
            playerCard.AbilitySystem.FindAllAbilitiesWithTags(
                handles,
                skillTags,
                exactMatch: false);

            List<AdventureSkillSlotViewModel> viewModels = new(handles.Count);
            for (int i = 0; i < handles.Count; i++)
            {
                GameplayAbilitySpecHandle handle = handles[i];
                if (!playerCard.AbilitySystem.TryGetAbilitySpec(handle, out GameplayAbilitySpec spec))
                    continue;

                if (spec.Ability is not SkillGameplayAbility skillAbility)
                    continue;

                viewModels.Add(new AdventureSkillSlotViewModel(
                    skillAbility.Name,
                    skillAbility.Icon,
                    handle,
                    skillAbility.TargetType));
            }

            return viewModels;
        }

        public bool UseSkill(GameplayAbilitySpecHandle handle)
        {
            Card playerCard = _playerService.GetPlayerCard();
            if (playerCard == null)
                return false;

            return playerCard.AbilitySystem.TryActivateAbility(handle);
        }

        public bool UseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            Card playerCard = _playerService.GetPlayerCard();
            if (playerCard == null)
                return false;

            if (!_cardService.TryGet(targetCardId, out Card targetCard))
                return false;

            GameplayEventData eventData = new(default)
            {
                Instigator = playerCard.AbilitySystem,
                Target = targetCard.AbilitySystem,
            };

            return playerCard.AbilitySystem.TriggerAbilityFromGameplayEvent(handle, eventData);
        }
    }
}
