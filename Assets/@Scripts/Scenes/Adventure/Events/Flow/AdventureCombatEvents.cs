using System;
using Domains.Combat;
using Domains.Intent.Presentation;
using UnityEngine;

namespace Game.Scenes.Adventure.Events.Flow
{
    // Role:
    // Carries combat presentation requests emitted by Adventure game flow.
    // Awaitable callbacks are used when game flow must wait for UI presentation completion.
    public sealed class AdventureCombatEvents
    {
        public Func<MonsterIntentRevealViewModel, Awaitable> IntentRefreshRequested;
        public Func<uint, Awaitable<bool>> IntentTriggeredRequested;
        public Func<ECombatEndResult, Awaitable<bool>> ResultRequested;
    }
}
