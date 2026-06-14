using System;
using System.Collections.Generic;
using Game.AbilitySystem.Abilities;
using UnityEngine;

namespace Domains.Combat
{
    public sealed class IntentComponent
    {
        private static readonly IReadOnlyList<Sprite> EmptyIcons = Array.Empty<Sprite>();

        public SkillGameplayAbility CurrentActionAbility { get; private set; }
        public IReadOnlyList<Sprite> CurrentIntentIcons { get; private set; } = EmptyIcons;

        public event Action<IntentComponent> IntentChanged;

        public void SetIntent(
            SkillGameplayAbility actionAbility,
            IReadOnlyList<Sprite> intentIcons)
        {
            CurrentActionAbility = actionAbility;
            CurrentIntentIcons = intentIcons ?? EmptyIcons;
            IntentChanged?.Invoke(this);
        }

        public void ClearIntent()
        {
            CurrentActionAbility = null;
            CurrentIntentIcons = EmptyIcons;
            IntentChanged?.Invoke(this);
        }
    }
}
