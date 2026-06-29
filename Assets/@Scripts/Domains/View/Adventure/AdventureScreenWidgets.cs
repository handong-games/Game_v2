using System;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Carries preloaded screen widget UXML templates for one AdventureScene scope.
    public sealed class AdventureScreenWidgetTemplates
    {
        public AdventureScreenWidgetTemplates(VisualTreeAsset skillSlot)
        {
            SkillSlot = skillSlot ?? throw new ArgumentNullException(nameof(skillSlot));
        }

        public VisualTreeAsset SkillSlot { get; }
    }

    // Role:
    // Holds top-level Adventure screen VisualElement references.
    // AdventureView initializes this once after the visual tree is cloned.
    public sealed class AdventureScreenWidgets
    {
        private const string AdventureRootName = "adventure-root";
        private const string BackgroundName = "adventure-background";
        private const string EmblemName = "adventure-emblem";
        private const string BannerName = "banner";
        private const string ResourceStatusBarName = "resource-status-bar";
        private const string ProgressBarName = "progress-bar";
        private const string ProgressBarTrackFillName = "progress-bar-track-fill";
        private const string EffectLayerName = "adventure-effect-layer";
        private const string CoinStatusWidgetName = "coin-status-widget";
        private const string EndTurnWidgetName = "end-turn-widget";
        private const string ArrowWidgetName = "arrow-widget";
        private const string CardDeckName = "card-deck";
        private const string SkillSlotGroupName = "skill-slot-group";

        private readonly AdventureBoardWidgets _boardWidgets;

        public AdventureScreenWidgets(AdventureBoardWidgets boardWidgets)
        {
            _boardWidgets = boardWidgets ?? throw new ArgumentNullException(nameof(boardWidgets));
        }

        public VisualElement Root { get; private set; }
        public VisualElement AdventureRoot { get; private set; }
        public VisualElement Background { get; private set; }
        public VisualElement Emblem { get; private set; }
        public Banner Banner { get; private set; }
        public ResourceStatusBar ResourceStatusBar { get; private set; }
        public VisualElement ProgressBar { get; private set; }
        public VisualElement ProgressBarTrackFill { get; private set; }
        public VisualElement EffectLayer { get; private set; }
        public Pouch Pouch => _boardWidgets.Pouch;
        public CoinStatusWidget CoinStatus { get; private set; }
        public EndTurnWidget EndTurn { get; private set; }
        public ArrowWidget Arrow { get; private set; }
        public VisualElement CardDeck { get; private set; }
        public AdventureSkillSlotGroup SkillSlots { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(VisualElement root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));

            AdventureRoot = FindRequired<VisualElement>(Root, AdventureRootName);
            Background = FindRequired<VisualElement>(Root, BackgroundName);
            Emblem = FindRequired<VisualElement>(Root, EmblemName);
            Banner = FindRequired<Banner>(Root, BannerName);
            ResourceStatusBar = FindRequired<ResourceStatusBar>(Root, ResourceStatusBarName);
            ProgressBar = FindRequired<VisualElement>(Root, ProgressBarName);
            ProgressBarTrackFill = FindRequired<VisualElement>(Root, ProgressBarTrackFillName);
            EffectLayer = FindRequired<VisualElement>(Root, EffectLayerName);
            CoinStatus = FindRequired<CoinStatusWidget>(Root, CoinStatusWidgetName);
            EndTurn = FindRequired<EndTurnWidget>(Root, EndTurnWidgetName);
            Arrow = FindRequired<ArrowWidget>(Root, ArrowWidgetName);
            CardDeck = FindRequired<VisualElement>(Root, CardDeckName);
            SkillSlots = FindRequired<AdventureSkillSlotGroup>(Root, SkillSlotGroupName);

            _boardWidgets.Initialize(Root);
            IsInitialized = true;
        }

        public void Clear()
        {
            _boardWidgets.Clear();

            Root = null;
            AdventureRoot = null;
            Background = null;
            Emblem = null;
            Banner = null;
            ResourceStatusBar = null;
            ProgressBar = null;
            ProgressBarTrackFill = null;
            EffectLayer = null;
            CoinStatus = null;
            EndTurn = null;
            Arrow = null;
            CardDeck = null;
            SkillSlots = null;
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
