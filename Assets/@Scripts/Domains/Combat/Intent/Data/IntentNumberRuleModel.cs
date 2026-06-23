using UnityEngine;
using Domains.Combat.Intent;

namespace Domains.Combat.Intent.Data
{
    public abstract class IntentNumberRuleModel : ScriptableObject
    {
        public abstract IntentNumberData BuildNumber();
    }
}
