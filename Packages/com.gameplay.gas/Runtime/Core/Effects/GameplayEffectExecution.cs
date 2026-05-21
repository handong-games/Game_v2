namespace Gameplay.GAS
{
    using System.Collections.Generic;

    public abstract class GameplayEffectExecution
    {
        public virtual void GetAttributeCaptureDefinitions(
            List<GameplayEffectAttributeCaptureDefinition> definitions)
        {
        }

        public abstract void Execute(
            GameplayEffectExecutionParameters parameters,
            GameplayEffectExecutionOutput output);
    }
}
