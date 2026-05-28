using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities.Warrior
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Warrior/GA Skill BasicAttack")]
    public sealed class GA_Skill_BasicAttack : SkillGameplayAbility
    {
        [SerializeField]
        private GameplayEffect _damageGameplayEffect;

        public GA_Skill_BasicAttack()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilitySkillWarriorBasicAttack);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
            if (!CheckCost(handle, actorInfo))
                return;

            AbilitySystemComponent target = triggerEventData?.ResolvedTarget;
            if (target == null)
                return;

            if (!CommitAbilityCost(handle, actorInfo, activationInfo))
                return;

            if (_damageGameplayEffect == null)
                return;

            actorInfo.AbilitySystem.ApplyGameplayEffectToTarget(
                _damageGameplayEffect,
                target,
                GetAbilityLevel(actorInfo, handle));
        }
    }
}
