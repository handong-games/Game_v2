using Gameplay.GAS;

namespace Game.AbilitySystem
{
    [GameplayTagProvider]
    public static class AbilityGameplayTags
    {
        public const string Ability_Skill = "Ability.Skill";
        public const string Ability_Skill_Warrior = "Ability.Skill.Warrior";
        public const string Ability_Skill_Warrior_BasicAttack = "Ability.Skill.Warrior.BasicAttack";
        public const string Ability_Skill_Warrior_Strike = "Ability.Skill.Warrior.Strike";
        public const string Ability_Skill_Warrior_Rally = "Ability.Skill.Warrior.Rally";
        public const string Ability_Skill_Warrior_Guard = "Ability.Skill.Warrior.Guard";
        public const string Ability_CoinFlip = "Ability.CoinFlip";
        public const string Event_CoinFlip = "Event.CoinFlip";
        public const string GameplayCue_CoinFlip = "GameplayCue.CoinFlip";
        public const string GameplayCue_CoinChange = "GameplayCue.CoinChange";
        public const string GameplayCue_Damage = "GameplayCue.Damage";

        public static readonly GameplayTag AbilitySkill = GameplayTag.Define(Ability_Skill);
        public static readonly GameplayTag AbilitySkillWarrior = GameplayTag.Define(Ability_Skill_Warrior);
        public static readonly GameplayTag AbilitySkillWarriorBasicAttack = GameplayTag.Define(Ability_Skill_Warrior_BasicAttack);
        public static readonly GameplayTag AbilitySkillWarriorStrike = GameplayTag.Define(Ability_Skill_Warrior_Strike);
        public static readonly GameplayTag AbilitySkillWarriorRally = GameplayTag.Define(Ability_Skill_Warrior_Rally);
        public static readonly GameplayTag AbilitySkillWarriorGuard = GameplayTag.Define(Ability_Skill_Warrior_Guard);
        public static readonly GameplayTag AbilityCoinFlip = GameplayTag.Define(Ability_CoinFlip);
        public static readonly GameplayTag EventCoinFlip = GameplayTag.Define(Event_CoinFlip);
        public static readonly GameplayTag GameplayCueCoinFlip = GameplayTag.Define(GameplayCue_CoinFlip);
        public static readonly GameplayTag GameplayCueCoinChange = GameplayTag.Define(GameplayCue_CoinChange);
        public static readonly GameplayTag GameplayCueDamage = GameplayTag.Define(GameplayCue_Damage);
    }
}
