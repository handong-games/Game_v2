#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using NUnit.Framework;
using UnityEngine;

namespace Game.AbilitySystem.Abilities.Tests
{
    public sealed class GA_CoinFlipTests
    {
        [Test]
        public void HandleGameplayEvent_FlipsCoinCountAndWritesHeadsAndTails()
        {
            GameplayActor actor = new();
            CombatAttributeSet combatSet = new();
            SetAttributeValue(combatSet.CoinCount, 3f);
            actor.AbilitySystem.AddAttributeSet(combatSet);

            PredictableCoinFlipAbility ability =
                ScriptableObject.CreateInstance<PredictableCoinFlipAbility>();
            ability.SetResults(true, false, true);
            actor.AbilitySystem.GiveAbility(ability);

            int triggeredCount = actor.AbilitySystem.HandleGameplayEvent(
                new GameplayEventData(AbilityGameplayTags.EventCoinFlip)
                {
                    Instigator = actor.AbilitySystem
                });

            Assert.That(triggeredCount, Is.EqualTo(1));
            Assert.That(combatSet.CoinHeads.CurrentValue, Is.EqualTo(2f));
            Assert.That(combatSet.CoinTails.CurrentValue, Is.EqualTo(1f));
        }

        [Test]
        public void HandleGameplayEvent_DoesNotPublishChanges_WhenAggregateResultIsUnchanged()
        {
            GameplayActor actor = new();
            CombatAttributeSet combatSet = new();
            SetAttributeValue(combatSet.CoinCount, 3f);
            SetAttributeValue(combatSet.CoinHeads, 2f);
            SetAttributeValue(combatSet.CoinTails, 1f);
            actor.AbilitySystem.AddAttributeSet(combatSet);

            int headsChangedCount = 0;
            int tailsChangedCount = 0;
            combatSet.CoinHeads.CurrentValueChanged += (_, _) => headsChangedCount++;
            combatSet.CoinTails.CurrentValueChanged += (_, _) => tailsChangedCount++;

            PredictableCoinFlipAbility ability =
                ScriptableObject.CreateInstance<PredictableCoinFlipAbility>();
            ability.SetResults(true, false, true);
            actor.AbilitySystem.GiveAbility(ability);

            actor.AbilitySystem.HandleGameplayEvent(
                new GameplayEventData(AbilityGameplayTags.EventCoinFlip)
                {
                    Instigator = actor.AbilitySystem
                });

            Assert.That(combatSet.CoinHeads.CurrentValue, Is.EqualTo(2f));
            Assert.That(combatSet.CoinTails.CurrentValue, Is.EqualTo(1f));
            Assert.That(headsChangedCount, Is.EqualTo(0));
            Assert.That(tailsChangedCount, Is.EqualTo(0));
        }

        private static void SetAttributeValue(GameplayAttributeData data, float value)
        {
            data.SetBaseValue(value);
            data.SetCurrentValue(value);
        }

        private sealed class PredictableCoinFlipAbility : GA_CoinFlip
        {
            private readonly Queue<bool> _results = new();

            public void SetResults(params bool[] results)
            {
                _results.Clear();
                for (int i = 0; i < results.Length; i++)
                {
                    _results.Enqueue(results[i]);
                }
            }

            protected override bool FlipCoin()
            {
                return _results.Dequeue();
            }
        }
    }
}
#endif
