using UnityEngine;
using Domains.Combat.Intent;

namespace Domains.Combat.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Combat/Intent/Fixed Number Rule")]
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
