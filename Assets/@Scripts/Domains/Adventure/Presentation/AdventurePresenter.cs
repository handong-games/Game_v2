using System;
using System.Collections.Generic;
using Domains.Card;
using Domains.Combat;
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
        private readonly AdventureCombatRuntime _combat;
        private readonly AdventureRegionData _region;

        public AdventurePresenter(
            AdventurePlayer player,
            AdventureCards cards,
            AdventureBoard board,
            AdventureStageRuntime stage,
            AdventureCombatRuntime combat,
            AdventureRegionData region)
        {
            _player = player;
            _cards = cards;
            _board = board;
            _stage = stage;
            _combat = combat;
            _region = region;
        }

        public AdventureEntryPresentationViewModel CreateEntryPresentation()
        {
            AdventureEntryPresentationModel presentation = _region.Adventure.EntryPresentation;
            return new AdventureEntryPresentationViewModel(
                presentation.Title,
                presentation.Subtitle,
                presentation.Background,
                presentation.Emblem);
        }

        public IReadOnlyList<AdventureCardViewModel> CreateBoardCards()
        {
            List<AdventureCardViewModel> cards = new();
            AddBoardZoneCards(cards, ECardZone.Left);
            AddBoardZoneCards(cards, ECardZone.Right);
            return cards;
        }

        public IReadOnlyList<AdventureBoardCardViewModel> CreateBoardDisplayCards()
        {
            return CreateBoardDisplayCards(CreateBoardCards());
        }

        public CombatTurnViewModel CreateCombatTurnViewModel()
        {
            return new CombatTurnViewModel(
                _combat.CurrentSide,
                _combat.RoundNumber);
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
                    continue;

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
