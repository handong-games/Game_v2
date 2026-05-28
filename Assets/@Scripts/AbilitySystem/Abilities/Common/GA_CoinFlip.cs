using Domains.Adventure;
using Domains.Player;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Game.AbilitySystem.Abilities
{
    [CreateAssetMenu(menuName = "Game/AbilitySystem/Abilities/Common/GA Coin Flip")]
    public class GA_CoinFlip : GameplayAbility
    {
        public GA_CoinFlip()
        {
            AbilityTags.AddTag(AbilityGameplayTags.AbilityCoinFlip);
            AddGameplayEventTrigger(AbilityGameplayTags.EventCoinFlip);
        }

        public override void ActivateAbility(
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            GameplayEventData triggerEventData)
        {
            CombatAttributeSet combatSet = actorInfo.AbilitySystem.GetSet<CombatAttributeSet>();
            if (combatSet == null)
                return;

            int coinCount = Mathf.Max(0, Mathf.RoundToInt(combatSet.CoinCount.CurrentValue));
            ECoinFace[] faces = new ECoinFace[coinCount];
            int heads = 0;
            int tails = 0;

            for (int i = 0; i < coinCount; i++)
            {
                bool isHeads = FlipCoin();
                faces[i] = isHeads ? ECoinFace.Heads : ECoinFace.Tails;

                if (isHeads)
                    heads++;
                else
                    tails++;
            }

            SetAttributeValue(combatSet.CoinHeads, heads);
            SetAttributeValue(combatSet.CoinTails, tails);

            GameplayEffectContext context = new(
                actorInfo.AbilitySystem,
                triggerEventData?.ResolvedTarget ?? actorInfo.AbilitySystem,
                new CoinFlipCueData(faces));

            actorInfo.AbilitySystem.ExecuteGameplayCue(
                AbilityGameplayTags.GameplayCueCoinFlip,
                new GameplayCueParameters(
                    context,
                    normalizedMagnitude: 1f,
                    rawMagnitude: coinCount,
                    abilityLevel: 1));
        }

        protected virtual bool FlipCoin()
        {
            return Random.value < 0.5f;
        }

        private static void SetAttributeValue(GameplayAttributeData data, float value)
        {
            data.SetBaseValue(value);
            data.SetCurrentValue(value);
        }
    }
}
