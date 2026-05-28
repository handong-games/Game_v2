using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    public abstract class GameplayEffectExecutionCalculation : ScriptableObject
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
