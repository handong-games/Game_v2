using System;
using Domains.Adventure;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Holds the Adventure board VisualElement references used by board UI flows.
    public sealed class AdventureBoardWidgets
    {
        private const string BoardName = "card-board";
        private const string LeftAreaName = "card-board-left-area";
        private const string CenterAreaName = "card-board-center-area";
        private const string RightAreaName = "card-board-right-area";
        private const string PouchName = "pouch";

        public VisualElement Root { get; private set; }
        public VisualElement LeftArea { get; private set; }
        public VisualElement CenterArea { get; private set; }
        public VisualElement RightArea { get; private set; }
        public Pouch Pouch { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(VisualElement screenRoot)
        {
            if (screenRoot == null)
                throw new ArgumentNullException(nameof(screenRoot));

            Root = FindRequired<VisualElement>(screenRoot, BoardName);
            LeftArea = FindRequired<VisualElement>(Root, LeftAreaName);
            CenterArea = FindRequired<VisualElement>(Root, CenterAreaName);
            RightArea = FindRequired<VisualElement>(Root, RightAreaName);
            Pouch = FindRequired<Pouch>(Root, PouchName);
            IsInitialized = true;
        }

        public VisualElement GetArea(AdventureBoardSide side)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AdventureBoardWidgets is not initialized.");

            return side switch
            {
                AdventureBoardSide.Left => LeftArea,
                AdventureBoardSide.Right => RightArea,
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
            };
        }

        public void Clear()
        {
            Root = null;
            LeftArea = null;
            CenterArea = null;
            RightArea = null;
            Pouch = null;
            IsInitialized = false;
        }

        private static T FindRequired<T>(VisualElement root, string name)
            where T : VisualElement
        {
            T element = root.Q<T>(name);
            if (element == null)
                throw new InvalidOperationException($"Required element missing: {name}");

            return element;
        }
    }
}
