using System;
using System.Collections.Generic;
using Domains.Adventure;

namespace Game.Scenes.Adventure.Events.Flow
{
    public sealed class AdventureBoardEvents
    {
        public Action<IReadOnlyList<AdventureCardViewModel>> RefreshRequested;
    }
}
