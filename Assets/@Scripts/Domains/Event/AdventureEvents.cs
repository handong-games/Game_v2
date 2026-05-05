using System;
using System.Collections.Generic;
using Domains.Adventure;

namespace Domains.Event
{
    /// <summary>
    /// Public static delegates associated with Adventure flow.
    ///
    /// These are "events" in the conceptual sense and not strict C# events.
    /// </summary>
    public static class AdventureEvents
    {
        public static Action AdventureStarted;
        public static Action IntroCompleted;
        public static Action<IReadOnlyList<CardState>> CardsDrawn;
        public static Action CardDealCompleted;
        public static Action TurnBannerRequested;
    }
}
