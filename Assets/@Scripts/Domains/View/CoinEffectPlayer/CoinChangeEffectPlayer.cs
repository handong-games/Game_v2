using System;
using Domains.Player;
using UnityEngine;

namespace Domains.Adventure
{
    public sealed class CoinChangeEffectPlayer
    {
        private const float ChangeStaggerSeconds = 0.12f;

        public async Awaitable Play(
            CoinChangeCueData cueData,
            Action<ECoinFace, int> onChanged)
        {
            if (cueData == null || !cueData.HasEntries)
                return;

            for (int i = 0; i < cueData.Entries.Count; i++)
            {
                CoinChangeCueEntry entry = cueData.Entries[i];
                onChanged?.Invoke(entry.Face, entry.Delta);
                await Awaitable.WaitForSecondsAsync(ChangeStaggerSeconds);
            }
        }
    }
}
