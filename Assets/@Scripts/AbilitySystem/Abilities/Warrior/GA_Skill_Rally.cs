using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities.Warrior
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Warrior/GA Skill Rally")]
    public sealed class GA_Skill_Rally : SkillGameplayAbility
    {
        public GA_Skill_Rally()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilitySkillWarriorRally);
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
