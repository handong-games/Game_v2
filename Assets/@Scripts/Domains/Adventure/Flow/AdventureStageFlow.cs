using System;
using System.Collections.Generic;
using Domains.Card;
using Game.Data;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Starts the current Adventure stage by drawing encounter entries and placing offer cards.
    // It does not create view models or play animations.
    public sealed class AdventureStageFlow
    {
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

        public AdventureStageFlow(
            AdventureRunState runState,
            AdventureRegionData region,
            AdventureProgress progress,
            AdventurePlayer player,
            AdventureCards cards,
            AdventureBoard board,
            AdventureStageRuntime stage,
            AdventureInputState input,
            AdventureEncounterSequenceRuntime encounterSequence,
            AdventureOfferFactory offerFactory)
        {
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
        }

        public void StartCurrentStage(bool removeExistingStageCards = true)
        {
            AdventureRun run = _runState.CurrentRun;
            if (run == null)
                throw new InvalidOperationException("AdventureRun is not initialized.");

            _progress.EnterStageStart();
            _stage.Clear();
            _input.Clear();
            IReadOnlyList<uint> existingStageCardIds = TakeStageBoardCardIds();
            if (removeExistingStageCards)
                _cards.RemoveAll(existingStageCardIds);

            if (_player.PlayerCard != null)
                _board.PlaceCard(ECardZone.Left, _player.PlayerCard.CardId);

            AdventureStageDefinition stage = GetStage(run, _region.Adventure);
            _stage.SetStartMode(stage?.StartMode ?? AdventureStageStartMode.Choice);

            int drawCount = GetStageDrawCount(run, _region.Adventure, stage);
            IReadOnlyList<AdventureEncounterSequenceEntry> entries = _encounterSequence.Draw(drawCount);

            for (int i = 0; i < entries.Count; i++)
            {
                AdventureEncounterSequenceEntry entry = entries[i];
                AdventureEncounterOffer offer = _offerFactory.Create(entry, entry.EncounterCard);
                CardModelBase encounterCard = offer.EncounterCard;
                CardActor card = _cards.Create(encounterCard);

                _stage.BindOfferCard(offer, card.CardId);
                _board.PlaceCard(ECardZone.Right, card.CardId);
            }

            if (_stage.StartMode == AdventureStageStartMode.Choice)
            {
                _progress.EnterChoice();
            }
        }

        public void OpenChoiceSelection()
        {
            if (_progress.CurrentPhase != AdventurePhase.Choice ||
                _stage.StartMode != AdventureStageStartMode.Choice)
            {
                return;
            }

            _input.EnterChoiceSelection();
        }

        private static AdventureStageDefinition GetStage(
            AdventureRun run,
            AdventureRegionModel adventure)
        {
            return adventure.TryGetStage(run.StageNumber, out AdventureStageDefinition stage)
                ? stage
                : null;
        }

        private static int GetStageDrawCount(
            AdventureRun run,
            AdventureRegionModel adventure,
            AdventureStageDefinition stage)
        {
            if (stage?.StartMode == AdventureStageStartMode.ImmediateEncounter)
                return 1;

            uint drawCount;
            if (stage != null)
            {
                drawCount = stage.DrawCount;
            }
            else if (run.StageNumber == 1)
            {
                drawCount = adventure.StartDrawCount;
            }
            else if (run.StageNumber >= run.MaxStageCount)
            {
                drawCount = 1;
            }
            else
            {
                drawCount = adventure.StartDrawCount;
            }

            return (int)Math.Max(1, drawCount);
        }

        public IReadOnlyList<uint> TakeStageBoardCardIds()
        {
            List<uint> cardIds = new();
            AddTakenCardIds(cardIds, ECardZone.Right);
            AddTakenCardIds(cardIds, ECardZone.Removed);
            return cardIds;
        }

        private void AddTakenCardIds(
            List<uint> cardIds,
            ECardZone zone)
        {
            IReadOnlyList<uint> takenCardIds = _board.TakeCardIds(zone);
            for (int i = 0; i < takenCardIds.Count; i++)
            {
                cardIds.Add(takenCardIds[i]);
            }
        }
    }
}
