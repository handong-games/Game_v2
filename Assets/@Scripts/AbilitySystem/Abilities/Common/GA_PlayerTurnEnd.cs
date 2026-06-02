using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Common/GA Player Turn End")]
    public sealed class GA_PlayerTurnEnd : GameplayAbility
    {
        public GA_PlayerTurnEnd()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilityCombatTurnEnd);
            AddGameplayEventTrigger(AbilityGameplayTags.EventCombatTurnEnded);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
            CostAttributeSet costSet = actorInfo.AbilitySystem.GetSet<CostAttributeSet>();
            if (costSet == null)
            {
                EndAbility(handle, actorInfo, activationInfo, wasCancelled: true);
                return;
            }

            costSet.RestoreCoinsToBase();
            EndAbility(handle, actorInfo, activationInfo, wasCancelled: false);
        }
    }
}
