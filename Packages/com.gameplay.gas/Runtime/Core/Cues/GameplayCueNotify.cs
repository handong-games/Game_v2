using System;

namespace Gameplay.GAS
{
    [Serializable]
    public abstract class GameplayCueNotify
    {
        public abstract void HandleGameplayCue(
            AbilitySystemComponent target,
            GameplayTag cueTag,
            GameplayCueEvent eventType,
            GameplayCueParameters parameters);
    }
}
