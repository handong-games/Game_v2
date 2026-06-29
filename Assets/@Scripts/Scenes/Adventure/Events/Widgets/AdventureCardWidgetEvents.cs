using System;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure.Events.Widgets
{
    // Role:
    // Carries card-widget pointer input from dynamic board cards to the Adventure screen.
    public sealed class AdventureCardWidgetEvents
    {
        public Action<VisualElement, uint> PointerEntered;
        public Action<VisualElement, uint> PointerLeft;
        public Action<VisualElement, uint> Clicked;
    }
}
