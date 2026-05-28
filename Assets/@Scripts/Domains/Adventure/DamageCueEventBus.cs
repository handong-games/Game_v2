using System;

namespace Domains.Adventure
{
    public static class DamageCueEventBus
    {
        public static event Action<DamageCueData> Published;

        public static void Publish(DamageCueData data)
        {
            Published?.Invoke(data);
        }
    }
}
