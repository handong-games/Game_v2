using System;
using System.Collections.Generic;
using Game.Data;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Combat.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Combat/Intent/Action")]
    public sealed class IntentActionModel : AbstractModel<EIntentAction>
    {
        [SerializeField]
        private GameplayAbility _executionAbility;

        [SerializeField]
        private IntentDisplayDefinition[] _displayDefinitions;

        public GameplayAbility ExecutionAbility => _executionAbility;

        public IReadOnlyList<IntentDisplayDefinition> DisplayDefinitions =>
            _displayDefinitions ?? Array.Empty<IntentDisplayDefinition>();
    }
}
