using System;
using System.Collections.Generic;
using Domains.Card;
using Game.Data;

namespace Domains.Adventure
{
    // Role:
    // Handles current-stage choice selection and commit.
    // It does not start encounter content or create view models.
    public sealed class AdventureChoiceFlow
    {
        private readonly AdventureInputState _input;
        private readonly AdventureStageRuntime _stage;
        private readonly AdventureBoard _board;

        public AdventureChoiceFlow(
            AdventureInputState input,
            AdventureStageRuntime stage,
            AdventureBoard board)
        {
            _input = input;
            _stage = stage;
            _board = board;
        }

        public AdventureChoiceSelectionResult Select(uint offerCardId)
        {
            if (_input.CurrentMode != AdventureInputMode.ChoiceSelection)
                return AdventureChoiceSelectionResult.None;

            if (!_stage.TryGetBindingByOfferCardId(offerCardId, out AdventureStageOfferBinding binding))
                return AdventureChoiceSelectionResult.None;

            _stage.SelectBinding(binding);
            _input.Clear();
            return new AdventureChoiceSelectionResult(true, offerCardId);
        }

        public AdventureChoiceCommitResult CommitSelection()
        {
            AdventureStageOfferBinding binding = _stage.SelectedBinding;
            if (binding == null)
                throw new InvalidOperationException("Choice selection was not made.");

            _board.MoveAllExcept(ECardZone.Right, binding.OfferCardId, ECardZone.Removed);

            return new AdventureChoiceCommitResult(
                binding.OfferCardId,
                binding.Offer.EncounterType,
                binding.Offer.EncounterCard);
        }

        public bool TryCommitImmediateEncounter(out AdventureChoiceCommitResult result)
        {
            result = default;

            if (_stage.StartMode != AdventureStageStartMode.ImmediateEncounter)
                return false;

            IReadOnlyList<AdventureStageOfferBinding> bindings = _stage.Bindings;
            if (bindings.Count == 0)
                return false;

            AdventureStageOfferBinding binding = bindings[0];
            if (binding == null)
                return false;

            _stage.SelectBinding(binding);
            _input.Clear();
            _board.MoveAllExcept(ECardZone.Right, binding.OfferCardId, ECardZone.Removed);

            result = new AdventureChoiceCommitResult(
                binding.OfferCardId,
                binding.Offer.EncounterType,
                binding.Offer.EncounterCard);
            return true;
        }
    }
}
