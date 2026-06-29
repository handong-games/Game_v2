using System;
using Domains.Combat;
using Game.Scenes.Adventure.Events.Flow;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Applies combat result to Adventure progression and notifies scene events.
    // It does not observe death messages or execute enemy actions.
    public sealed class AdventureCombatResultFlow
    {
        private readonly AdventureProgress _progress;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureScreenEvents _screenEvents;
        private readonly AdventureCombatEvents _combatEvents;
        private readonly AdventureEnemyActionFlow _enemyActionFlow;
        private readonly AdventureStageContinuationFlow _stageContinuationFlow;
        private bool _rewardCompletionPending;

        public AdventureCombatResultFlow(
            AdventureProgress progress,
            AdventureCombatRuntime combat,
            AdventureScreenEvents screenEvents,
            AdventureCombatEvents combatEvents,
            AdventureEnemyActionFlow enemyActionFlow,
            AdventureStageContinuationFlow stageContinuationFlow)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _screenEvents = screenEvents ?? throw new ArgumentNullException(nameof(screenEvents));
            _combatEvents = combatEvents ?? throw new ArgumentNullException(nameof(combatEvents));
            _enemyActionFlow = enemyActionFlow ?? throw new ArgumentNullException(nameof(enemyActionFlow));
            _stageContinuationFlow = stageContinuationFlow ?? throw new ArgumentNullException(nameof(stageContinuationFlow));
        }

        public async Awaitable CompleteCombat(ECombatEndResult result)
        {
            if (_combat.IsEnded)
                return;

            _combat.EndCombat();
            _enemyActionFlow.CancelExecution();

            switch (result)
            {
                case ECombatEndResult.Victory:
                    _progress.EnterReward();
                    _rewardCompletionPending = true;
                    AdventureRewardUIResult rewardResult = await NotifyRewardStarted();
                    if (!rewardResult.Completed)
                    {
                        _rewardCompletionPending = false;
                        return;
                    }

                    await CompleteReward(rewardResult.ClaimedRewardIds);
                    break;
                case ECombatEndResult.Defeat:
                    _progress.EnterDefeat();
                    _rewardCompletionPending = false;
                    await NotifyResult(result);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private async Awaitable CompleteReward(System.Collections.Generic.IReadOnlyList<uint> claimedRewardIds)
        {
            if (!_rewardCompletionPending)
                return;

            _rewardCompletionPending = false;
            await _stageContinuationFlow.ContinueAfterReward(claimedRewardIds);
        }

        private async Awaitable<bool> NotifyResult(ECombatEndResult result)
        {
            if (_combatEvents.ResultRequested == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureCombatEvents.ResultRequested)} is not bound.");

            return await _combatEvents.ResultRequested.Invoke(result);
        }

        private async Awaitable<AdventureRewardUIResult> NotifyRewardStarted()
        {
            if (_screenEvents.RewardStarted == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureScreenEvents.RewardStarted)} is not bound.");

            return await _screenEvents.RewardStarted.Invoke(new AdventureRewardViewModel());
        }

    }
}
