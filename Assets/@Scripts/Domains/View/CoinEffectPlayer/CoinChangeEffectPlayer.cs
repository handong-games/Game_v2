using System;
using Domains.Player;
using UnityEngine;

namespace Domains.Adventure
{
    public sealed class CoinChangeEffectPlayer
    {
        private const float ChangeStaggerSeconds = 0.12f;
        private int _playVersion;

        public async Awaitable Play(
            CoinChangeCueData cueData,
            Action<ECoinFace, int> onChanged)
        {
            if (cueData == null || !cueData.HasEntries)
                return;

            int playVersion = ++_playVersion;
            for (int i = 0; i < cueData.Entries.Count; i++)
            {
                if (playVersion != _playVersion)
                    return;

                CoinChangeCueEntry entry = cueData.Entries[i];
                onChanged?.Invoke(entry.Face, entry.Delta);
                await Awaitable.WaitForSecondsAsync(ChangeStaggerSeconds);
            }
        }

        public void Clear()
        {
            _playVersion++;
        }
    }
}
