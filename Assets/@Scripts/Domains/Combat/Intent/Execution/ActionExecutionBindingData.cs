using Domains.Combat.Intent.Data;
using Gameplay.GAS;

namespace Domains.Combat.Intent.Execution
{
    public readonly struct ActionExecutionBindingData
    {
        public ActionExecutionBindingData(
            IntentActionModel actionModel,
            GameplayAbilitySpecHandle abilitySpecHandle)
        {
            ActionModel = actionModel;
            AbilitySpecHandle = abilitySpecHandle;
        }

        public IntentActionModel ActionModel { get; }
        public GameplayAbilitySpecHandle AbilitySpecHandle { get; }
    }
}
