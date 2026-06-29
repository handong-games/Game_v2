using System;
using Domains.Intent.Execution;
using Domains.Intent.Runtime;
using Game.Scenes.Adventure;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Initializes Adventure runtime state when AdventureScene starts.
    // It does not start stages or run UI animation.
    public sealed class AdventureStartFlow
    {
        private readonly AdventureSceneInitialData _initialData;
        private readonly AdventureRunState _runState;
        private readonly AdventureRegionData _region;
        private readonly AdventureProgress _progress;
        private readonly AdventurePlayer _player;
        private readonly AdventureCards _cards;
        private readonly AdventureBoard _board;
        private readonly AdventureStageRuntime _stage;
        private readonly AdventureInputState _input;
        private readonly AdventureEncounterSequenceRuntime _encounterSequence;
        private readonly AdventureOfferFactory _offerFactory;
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureCardAvatarRegistry _avatarRegistry;
        private readonly IntentRuntime _intentRuntime;
        private readonly ActionExecutionBindingStore _actionBindings;

        public AdventureStartFlow(
            AdventureSceneInitialData initialData,
            AdventureRunState runState,
            AdventureRegionData region,
            AdventureProgress progress,
            AdventurePlayer player,
            AdventureCards cards,
            AdventureBoard board,
            AdventureStageRuntime stage,
            AdventureInputState input,
            AdventureEncounterSequenceRuntime encounterSequence,
            AdventureOfferFactory offerFactory,
            AdventureCombatRuntime combat,
            AdventureCardAvatarRegistry avatarRegistry,
            IntentRuntime intentRuntime,
            ActionExecutionBindingStore actionBindings)
        {
            _initialData = initialData ?? throw new ArgumentNullException(nameof(initialData));
            _runState = runState ?? throw new ArgumentNullException(nameof(runState));
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _encounterSequence = encounterSequence ?? throw new ArgumentNullException(nameof(encounterSequence));
            _offerFactory = offerFactory ?? throw new ArgumentNullException(nameof(offerFactory));
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _avatarRegistry = avatarRegistry ?? throw new ArgumentNullException(nameof(avatarRegistry));
            _intentRuntime = intentRuntime ?? throw new ArgumentNullException(nameof(intentRuntime));
            _actionBindings = actionBindings ?? throw new ArgumentNullException(nameof(actionBindings));
        }

        public void InitializeRuntime()
        {
            ClearRuntime();

            AdventureRun run = new(
                _initialData.SelectedCharacterId,
                _initialData.Adventure.Id,
                _initialData.Adventure.MaxStageCount,
                _initialData.Seed);

            _runState.SetCurrent(run);
            _region.Initialize(_initialData.Adventure);

            CardActor playerCard = _cards.Create(_initialData.Character);
            _player.Initialize(_initialData.Character, playerCard);

            _encounterSequence.Initialize(_initialData.Adventure, _initialData.Seed);
            _progress.EnterIntro();
        }

        private void ClearRuntime()
        {
            _avatarRegistry.Clear();
            _intentRuntime.Clear();
            _actionBindings.Clear();
            _combat.Clear();
            _offerFactory.Clear();
            _encounterSequence.Clear();
            _input.Clear();
            _stage.Clear();
            _board.Clear();
            _cards.Clear();
            _player.Clear();
            _progress.Clear();
            _runState.Clear();
        }
    }
}
