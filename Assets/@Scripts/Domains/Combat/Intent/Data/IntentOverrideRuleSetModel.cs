using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domains.Combat.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Combat/Intent/Override Rule Set")]
    public sealed class IntentOverrideRuleSetModel : ScriptableObject
    {
        [SerializeField]
        private IntentOverrideRule[] _rules;

        public IReadOnlyList<IntentOverrideRule> Rules =>
            _rules ?? Array.Empty<IntentOverrideRule>();
    }
}
