using System;
using System.Collections.Generic;
using Domains.Combat;
using Domains.Intent.Presentation;

namespace Game.Scenes.Adventure.Events.Flow
{
    public sealed class AdventureCombatEvents
    {
        public Action PlayerTurnBannerRequested;
        public Action<IReadOnlyList<MonsterIntentRevealViewModel>> IntentRevealRequested;
        public Action<uint> IntentTriggeredRequested;
        public Action EnemyTurnCompleted;
        public Action<ECombatEndResult> ResultRequested;
    }
}
