using System;

namespace Domains.Adventure
{
    public static class CoinFlipCueEventBus
    {
        public static event Action<CoinFlipCueData> Published;

        public static void Publish(CoinFlipCueData data)
        {
            Published?.Invoke(data);
        }
    }
}
