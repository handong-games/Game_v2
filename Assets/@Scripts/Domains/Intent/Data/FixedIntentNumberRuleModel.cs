using Domains.Intent.Runtime;
using UnityEngine;

namespace Domains.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Intent/Fixed Number Rule")]
    public sealed class FixedIntentNumberRuleModel : IntentNumberRuleModel
    {
        [SerializeField]
        private int _numberValue;

        public override IntentNumberData BuildNumber()
        {
            return new IntentNumberData(_numberValue, 1);
        }
    }
}
