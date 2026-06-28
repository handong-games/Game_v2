using Domains.Intent.Data;
using Gameplay.GAS;

namespace Domains.Intent.Execution
{
    public readonly struct ActionExecutionBindingData
    {
        public ActionExecutionBindingData(
            MonsterActionModel actionModel,
            GameplayAbilitySpecHandle abilitySpecHandle)
        {
            ActionModel = actionModel;
            AbilitySpecHandle = abilitySpecHandle;
        }

        public MonsterActionModel ActionModel { get; }
        public GameplayAbilitySpecHandle AbilitySpecHandle { get; }
    }
}
