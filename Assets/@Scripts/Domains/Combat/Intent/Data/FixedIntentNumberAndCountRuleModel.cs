using UnityEngine;
using Domains.Combat.Intent;

namespace Domains.Combat.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Combat/Intent/Fixed Number And Count Rule")]
    public sealed class FixedIntentNumberAndCountRuleModel : IntentNumberRuleModel
    {
        [SerializeField]
        private int _numberValue;

        [SerializeField]
        private int _countValue = 1;

        public override IntentNumberData BuildNumber()
        {
            return new IntentNumberData(_numberValue, _countValue);
        }
    }
}
