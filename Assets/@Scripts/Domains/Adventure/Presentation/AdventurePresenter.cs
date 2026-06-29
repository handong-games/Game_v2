using System;
using System.Collections.Generic;
using Domains.Card;
using Domains.View.Widgets;
using Game.AbilitySystem;
using Game.AbilitySystem.Abilities;
using Game.Data;
using Gameplay.GAS;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Converts Adventure runtime state into UI view models.
    // It does not mutate runtime, publish events, or execute gameplay.
    public sealed class AdventurePresenter
    {
        private readonly AdventurePlayer _player;
        private readonly AdventureCards _cards;
        private readonly AdventureBoard _board;
        private readonly AdventureStageRuntime _stage;
        private readonly AdventureRegionData _region;
        private readonly AdventureProgress _progress;

        public AdventurePresenter(
            AdventurePlayer player,
            AdventureCards cards,
            AdventureBoard board,
            AdventureStageRuntime stage,
            AdventureRegionData region,
            AdventureProgress progress)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _stage = stage ?? throw new ArgumentNullException(nameof(stage));
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public AdventureEntryPresentationViewModel CreateEntryPresentation()
        {
            AdventureEntryPresentationModel presentation = _region.Adventure.EntryPresentation;
            if (presentation == null)
                throw new InvalidOperationException("Adventure entry presentation is missing.");

            return new AdventureEntryPresentationViewModel(
                _region.Adventure.LocalizedRegionName,
                presentation.Title,
                presentation.Subtitle,
                presentation.Background,
                presentation.Emblem);
        }

        public AdventureInitialPresentationViewModel CreateInitialPresentation()
        {
            return new AdventureInitialPresentationViewModel(
                CreateEntryPresentation(),
                CreateResourceStatus(),
                CreateBoardPresentation(),
                CreateSkillSlots());
        }

        public AdventureResourceStatusViewModel CreateResourceStatus()
        {
            return new AdventureResourceStatusViewModel(
                _region.Adventure.LocalizedRegionName);
        }

        public AdventureBoardPresentationViewModel CreateBoardPresentation()
        {
            IReadOnlyList<AdventureCardViewModel> runtimeCards = CreateBoardCards();
            return new AdventureBoardPresentationViewModel(
                CreateBoardDisplayCards(runtimeCards),
                runtimeCards);
        }

        private IReadOnlyList<AdventureCardViewModel> CreateBoardCards()
        {
            List<AdventureCardViewModel> cards = new();
            AddBoardZoneCards(cards, ECardZone.Left);
            AddBoardZoneCards(cards, ECardZone.Right);
            return cards;
        }

        public IReadOnlyList<AdventureSkillSlotViewModel> CreateSkillSlots()
        {
            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return Array.Empty<AdventureSkillSlotViewModel>();

            GameplayTagContainer skillTags = new();
            skillTags.AddTag(AbilityGameplayTags.AbilitySkill);

            List<GameplayAbilitySpecHandle> handles = new();
            playerCard.AbilitySystem.FindAllAbilitiesWithTags(
                handles,
                skillTags,
                exactMatch: false);

            List<AdventureSkillSlotViewModel> viewModels = new(handles.Count);
            for (int i = 0; i < handles.Count; i++)
            {
                GameplayAbilitySpecHandle handle = handles[i];
                if (!playerCard.AbilitySystem.TryGetAbilitySpec(handle, out GameplayAbilitySpec spec))
                    continue;

                if (spec.Ability is not SkillGameplayAbility skillAbility)
                    continue;

                viewModels.Add(new AdventureSkillSlotViewModel(
                    skillAbility.Name,
                    skillAbility.Icon,
                    handle,
                    skillAbility.TargetType,
                    skillAbility));
            }

            return viewModels;
        }

        private void AddBoardZoneCards(List<AdventureCardViewModel> result, ECardZone zone)
        {
            IReadOnlyList<uint> cardIds = _board.GetCardIds(zone);
            for (int i = 0; i < cardIds.Count; i++)
            {
                uint cardId = cardIds[i];
                if (!_cards.TryGet(cardId, out CardActor card))
                    throw new InvalidOperationException(
                        $"Adventure board contains card id {cardId}, but AdventureCards does not contain the runtime card.");

                result.Add(CreateCardViewModel(card, zone));
            }
        }

        private AdventureCardViewModel CreateCardViewModel(CardActor card, ECardZone zone)
        {
            if (_stage.TryGetBindingByOfferCardId(card.CardId, out AdventureStageOfferBinding binding) &&
                binding.Offer.PreviewCard != card.Model)
            {
                return new AdventureCardViewModel(
                    card.CardId,
                    zone,
                    CardViewModelFactory.Create(
                        binding.Offer.PreviewCard,
                        CardViewModelFactory.GetDefaultFace(binding.Offer.PreviewCard)),
                    card.AbilitySystem);
            }

            return new AdventureCardViewModel(
                card.CardId,
                zone,
                CardViewModelFactory.Create(card),
                card.AbilitySystem);
        }

        private IReadOnlyList<AdventureBoardCardViewModel> CreateBoardDisplayCards(
            IReadOnlyList<AdventureCardViewModel> cards)
        {
            List<AdventureBoardCardViewModel> viewModels = new(cards.Count);
            for (int i = 0; i < cards.Count; i++)
            {
                viewModels.Add(CreateBoardDisplayCard(cards[i]));
            }

            return viewModels;
        }

        private AdventureBoardCardViewModel CreateBoardDisplayCard(
            AdventureCardViewModel card)
        {
            if (card.Zone == ECardZone.Right &&
                ShouldDisplayChoiceCard() &&
                _stage.TryGetBindingByOfferCardId(card.CardId, out AdventureStageOfferBinding binding) &&
                TryGetChoiceType(binding.Offer, out EChoiceCardType choiceType))
            {
                return new AdventureChoiceCardViewModel(
                    binding.OfferCardId,
                    choiceType);
            }

            return new AdventureBoardCardViewModel(
                ToBoardSide(card.Zone),
                card.CardId,
                card.Card);
        }

        private bool ShouldDisplayChoiceCard()
        {
            return _progress.CurrentPhase == AdventurePhase.Choice;
        }

        private static AdventureBoardSide ToBoardSide(ECardZone zone)
        {
            return zone switch
            {
                ECardZone.Left => AdventureBoardSide.Left,
                ECardZone.Right => AdventureBoardSide.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, null),
            };
        }

        private static bool TryGetChoiceType(
            AdventureEncounterOffer offer,
            out EChoiceCardType choiceType)
        {
            if (offer.PreviewCard.TryGetChoiceType(out choiceType))
                return true;

            choiceType = offer.EncounterType switch
            {
                AdventureEncounterType.Combat => EChoiceCardType.Monster,
                AdventureEncounterType.Boss => EChoiceCardType.Boss,
                AdventureEncounterType.Event => EChoiceCardType.Event,
                AdventureEncounterType.Shop => EChoiceCardType.Shop,
                _ => default,
            };

            return offer.EncounterType is
                AdventureEncounterType.Combat or
                AdventureEncounterType.Boss or
                AdventureEncounterType.Event or
                AdventureEncounterType.Shop;
        }
    }
}
