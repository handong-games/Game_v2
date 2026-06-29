using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Continues Adventure after reward UI is completed.
    // It advances the stage and starts immediate encounters when the next stage requires it.
    public sealed class AdventureStageContinuationFlow
    {
        private readonly AdventureStageAdvanceFlow _stageAdvanceFlow;
        private readonly AdventureEncounterStartFlow _encounterStartFlow;

        public AdventureStageContinuationFlow(
            AdventureStageAdvanceFlow stageAdvanceFlow,
            AdventureEncounterStartFlow encounterStartFlow)
        {
            _stageAdvanceFlow = stageAdvanceFlow ?? throw new ArgumentNullException(nameof(stageAdvanceFlow));
            _encounterStartFlow = encounterStartFlow ?? throw new ArgumentNullException(nameof(encounterStartFlow));
        }

        public async Awaitable ContinueAfterReward(IReadOnlyList<uint> claimedRewardIds)
        {
            _ = claimedRewardIds;

            AdventureStageAdvanceStatus status = await _stageAdvanceFlow.AdvanceOrComplete();
            if (status != AdventureStageAdvanceStatus.Advanced)
                return;

            await _encounterStartFlow.TryStartImmediateEncounter();
        }
    }
}
