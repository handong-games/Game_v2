using Domains.Intent.Runtime;
using UnityEngine;

namespace Domains.Intent.Data
{
    // Role:
    // Authored rule that calculates the number shown on an intent display item.
    public abstract class IntentNumberRuleModel : ScriptableObject
    {
        public abstract IntentNumberData BuildNumber();
    }
}
