using System;
using Gameplay.GAS;

namespace Domains.Adventure
{
    [Serializable]
    public sealed class CoinFlipGameplayCueNotify : GameplayCueNotify
    {
        public override void HandleGameplayCue(
            AbilitySystemComponent target,
            GameplayTag cueTag,
            GameplayCueEvent eventType,
            GameplayCueParameters parameters)
        {
            if (eventType != GameplayCueEvent.Executed || parameters?.Context == null)
                return;

            if (!parameters.Context.TryGetSourceObject(out CoinFlipCueData data))
                return;

            IAdventureCoinFlipCueReceiver receiver =
                target.GetAvatar<IAdventureCoinFlipCueReceiver>();

            receiver?.HandleCoinFlipCue(data);
        }
    }
}
