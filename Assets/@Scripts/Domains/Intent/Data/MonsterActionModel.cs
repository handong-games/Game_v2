using System;
using System.Collections.Generic;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Intent.Data
{
    // Role:
    // Authored monster action definition. It owns both execution Ability and intent display definitions.
    [CreateAssetMenu(menuName = "Game/Monster/Action")]
    public sealed class MonsterActionModel : ScriptableObject
    {
        [SerializeField]
        private GameplayAbility _executionAbility;

        [SerializeField]
        private IntentDisplayDefinition[] _intentDisplays;

        public GameplayAbility ExecutionAbility => _executionAbility;

        public IReadOnlyList<IntentDisplayDefinition> IntentDisplays =>
            _intentDisplays ?? Array.Empty<IntentDisplayDefinition>();
    }
}
