using System;
using UnityEngine;

namespace Domains.Combat.Intent.Data
{
    [Serializable]
    public sealed class IntentDisplayDefinition
    {
        [SerializeField]
        private IntentDisplayModel _displayModel;

        [SerializeField]
        private IntentNumberRuleModel _numberRule;

        public IntentDisplayModel DisplayModel => _displayModel;
        public IntentNumberRuleModel NumberRule => _numberRule;
    }
}
