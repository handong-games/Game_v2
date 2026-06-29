using System;
using Domains.Adventure;
using UnityEngine;

namespace Game.Scenes.Adventure.Events.Flow
{
    // Role:
    // Events targeted at the Adventure screen as a whole.
    // These are controller/game-flow outputs, not widget input events.
    public sealed class AdventureScreenEvents
    {
        public Func<AdventureInitialPresentationViewModel, Awaitable<bool>> InitialPresentationPrepared;
        public Func<AdventurePlayerTurnStartViewModel, Awaitable<bool>> PlayerTurnStarted;
        public Func<AdventureEnemyTurnStartViewModel, Awaitable<bool>> EnemyTurnStarted;
        public Func<AdventureRewardViewModel, Awaitable<AdventureRewardUIResult>> RewardStarted;
        public Func<AdventureChoiceRefreshViewModel, Awaitable<bool>> ChoiceRefreshStarted;
    }
}
