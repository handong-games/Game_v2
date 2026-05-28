using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities.Warrior
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Warrior/GA Skill Guard")]
    public sealed class GA_Skill_Guard : SkillGameplayAbility
    {
        public GA_Skill_Guard()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilitySkillWarriorGuard);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
        }
    }
}
