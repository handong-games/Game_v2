using Domains.Intent.Runtime;
using UnityEngine;

namespace Domains.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Intent/Fixed Number And Count Rule")]
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
