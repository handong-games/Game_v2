using System;

namespace Domains.Adventure
{
    public static class CoinChangeCueEventBus
    {
        public static event Action<CoinChangeCueData> Published;

        public static void Publish(CoinChangeCueData data)
        {
            Published?.Invoke(data);
        }
    }
}
