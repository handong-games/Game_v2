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
            AdventureCardAvatarRegistry avatarRegistry)
        {
            _initialData = initialData;
            _runState = runState;
            _region = region;
            _progress = progress;
            _player = player;
            _cards = cards;
            _board = board;
            _stage = stage;
            _input = input;
            _encounterSequence = encounterSequence;
            _offerFactory = offerFactory;
            _combat = combat;
            _avatarRegistry = avatarRegistry;
        }

        public void StartAdventure()
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
