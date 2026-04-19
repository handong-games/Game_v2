using System;
using UnityEngine;

namespace Game.Core.Managers.Event
{
    public static class Events
    {
        public static class Screen
        {
            public static Action<Vector2Int> ResolutionChanged;
        }
    }
}