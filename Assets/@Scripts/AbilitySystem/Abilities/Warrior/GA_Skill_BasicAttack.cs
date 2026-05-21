using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities.Warrior
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Warrior/GA Skill BasicAttack")]
    public sealed class GA_Skill_BasicAttack : SkillGameplayAbility
    {
        public GA_Skill_BasicAttack()
        {
            AbilityTags.AddTag(AbilityGameplayTags.Ability_Skill_Warrior_BasicAttack);
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
