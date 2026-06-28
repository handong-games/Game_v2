using System;
using UnityEngine;

namespace Domains.Intent.Data
{
    // Role:
    // Connects authored display data with an optional number rule for one monster action.
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
