using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domains.Intent.Data
{
    // Role:
    // Authored collection of override rules used when resolving monster intent actions.
    [CreateAssetMenu(menuName = "Game/Intent/Override Rule Set")]
    public sealed class IntentOverrideRuleSetModel : ScriptableObject
    {
        [SerializeField]
        private IntentOverrideRule[] _rules;

        public IReadOnlyList<IntentOverrideRule> Rules =>
            _rules ?? Array.Empty<IntentOverrideRule>();
    }
}
