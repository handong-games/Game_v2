using System;
using Domains.Adventure;
using UnityEngine;

namespace Game.Scenes.Adventure.Events.Flow
{
    // Role:
    // Board presentation events emitted by adventure game flow/controller.
    // Awaitable callbacks are used when game flow must wait for board transition completion.
    public sealed class AdventureBoardEvents
    {
        public Func<AdventureBoardPresentationViewModel, Awaitable<bool>> RefreshRequested;
        public Func<AdventureBoardCardRemoveViewModel, Awaitable<bool>> CardRemoveRequested;
        public Func<AdventureBoardCardDeathViewModel, Awaitable<bool>> CardDeathRequested;
        public Func<AdventureBoardSideClearViewModel, Awaitable<bool>> SideClearRequested;
    }
}
