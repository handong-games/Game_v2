using System;
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Adventure
{
    [Serializable]
    public sealed class DamageGameplayCueNotify : GameplayCueNotify
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

            int totalDamage = 0;

            for (int i = 0; i < modifiedAttributes.Count; i++)
            {
                GameplayEffectModifiedAttributeData modifiedAttribute = modifiedAttributes[i];
                if (!modifiedAttribute.Attribute.Equals(VitalAttributeSet.IncomingDamageAttribute))
                    continue;

                totalDamage += Mathf.RoundToInt(modifiedAttribute.TotalMagnitude);
            }

            if (totalDamage <= 0)
                return;

            DamageCueEventBus.Publish(new DamageCueData(totalDamage));
        }
    }
}
