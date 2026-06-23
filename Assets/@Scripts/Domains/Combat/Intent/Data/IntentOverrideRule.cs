using System;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Combat.Intent.Data
{
    [Serializable]
    public sealed class IntentOverrideRule
    {
        [SerializeField]
        private GameplayTag _conditionTag;

        [SerializeField]
        private IntentActionModel _replacementAction;

        [SerializeField]
        private int _priority;

        [SerializeField]
        private bool _advance;

        public GameplayTag ConditionTag => _conditionTag;
        public IntentActionModel ReplacementAction => _replacementAction;
        public int Priority => _priority;
        public bool Advance => _advance;
    }
}
