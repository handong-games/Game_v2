using System;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Intent.Data
{
    // Role:
    // Replaces a monster action when a matching gameplay tag condition is present.
    [Serializable]
    public sealed class IntentOverrideRule
    {
        [SerializeField]
        private GameplayTag _conditionTag;

        [SerializeField]
        private MonsterActionModel _replacementAction;

        [SerializeField]
        private int _priority;

        [SerializeField]
        private bool _advance;

        public GameplayTag ConditionTag => _conditionTag;
        public MonsterActionModel ReplacementAction => _replacementAction;
        public int Priority => _priority;
        public bool Advance => _advance;
    }
}
