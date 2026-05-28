using Game.AbilitySystem.Abilities;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities.Warrior
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Warrior/GA Skill Strike")]
    public sealed class GA_Skill_Strike : SkillGameplayAbility
    {
        public GA_Skill_Strike()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilitySkillWarriorStrike);
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
