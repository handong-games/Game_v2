using System;
using System.Collections.Generic;
using Domains.Player;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Adventure
{
    [Serializable]
    public sealed class CoinChangeGameplayCueNotify : GameplayCueNotify
    {
        public override void HandleGameplayCue(
            AbilitySystemComponent target,
            GameplayTag cueTag,
            GameplayCueEvent eventType,
            GameplayCueParameters parameters)
        {
            if (eventType != GameplayCueEvent.Executed || parameters?.Context == null)
                return;

            if (!parameters.Context.TryGetSourceObject(
                    out IReadOnlyList<GameplayEffectModifiedAttributeData> modifiedAttributes))
            {
                return;
            }

            List<CoinChangeCueEntry> entries = new();

            for (int i = 0; i < modifiedAttributes.Count; i++)
            {
                GameplayEffectModifiedAttributeData modifiedAttribute = modifiedAttributes[i];

                if (modifiedAttribute.Attribute.Equals(CombatAttributeSet.CoinHeadsAttribute))
                {
                    entries.Add(new CoinChangeCueEntry(
                        ECoinFace.Heads,
                        Mathf.RoundToInt(modifiedAttribute.TotalMagnitude)));
                }
                else if (modifiedAttribute.Attribute.Equals(CombatAttributeSet.CoinTailsAttribute))
                {
                    entries.Add(new CoinChangeCueEntry(
                        ECoinFace.Tails,
                        Mathf.RoundToInt(modifiedAttribute.TotalMagnitude)));
                }
            }

            if (entries.Count == 0)
                return;

            CoinChangeCueEventBus.Publish(new CoinChangeCueData(entries));
        }
    }
}
