using System;
using System.Collections.Generic;
using Domains.Card;
using Domains.Combat;
using Game.Core.Utility;
using Game.Data;
using Game.Generated;
using Gameplay.GAS;
using CardActor = Domains.Card.Card;
using UnityRandom = Unity.Mathematics.Random;

namespace Domains.Adventure
{
    public enum AdventurePhase
    {
        Intro,
        StageStart,
        Choice,
        Combat,
        Event,
        Shop,
        Reward,
        Complete,
        Defeat,
    }

    public enum AdventureInputMode
    {
        None,
        ChoiceSelection,
        SkillTargetSelection,
    }

    public enum AdventureEncounterType
    {
        Combat,
        Boss,
        Event,
        Shop,
    }

    // Role:
    // Stores Adventure progression phase for the current scene.
    // It does not create cards, publish events, or access UI.
    public sealed class AdventureProgress
    {
        public bool IntroCompleted { get; private set; }
        public AdventurePhase CurrentPhase { get; private set; } = AdventurePhase.Intro;

        public void MarkIntroCompleted() => IntroCompleted = true;
        public void EnterIntro() => CurrentPhase = AdventurePhase.Intro;
        public void EnterStageStart() => CurrentPhase = AdventurePhase.StageStart;
        public void EnterChoice() => CurrentPhase = AdventurePhase.Choice;
        public void EnterCombat() => CurrentPhase = AdventurePhase.Combat;
        public void EnterEvent() => CurrentPhase = AdventurePhase.Event;
        public void EnterShop() => CurrentPhase = AdventurePhase.Shop;
        public void EnterReward() => CurrentPhase = AdventurePhase.Reward;
        public void EnterComplete() => CurrentPhase = AdventurePhase.Complete;
        public void EnterDefeat() => CurrentPhase = AdventurePhase.Defeat;

        public void Clear()
        {
            IntroCompleted = false;
            CurrentPhase = AdventurePhase.Intro;
        }
    }

    // Role:
    // Holds the loaded Adventure region definition for scene-scoped flows.
    // It does not own mutable stage, board, or combat state.
    public sealed class AdventureRegionData
    {
        public AdventureRegionModel Adventure { get; private set; }

        public void Initialize(AdventureRegionModel adventure)
        {
            Adventure = adventure ?? throw new ArgumentNullException(nameof(adventure));
        }

        public void Clear()
        {
            Adventure = null;
        }
    }

    // Role:
    // Stores shared card-click input mode for Adventure.
    // It decides whether a card click means choice selection or skill targeting.
    public sealed class AdventureInputState
    {
        public AdventureInputMode CurrentMode { get; private set; }
        public GameplayAbilitySpecHandle SelectedSkillHandle { get; private set; } =
            GameplayAbilitySpecHandle.Invalid;

        public void EnterChoiceSelection()
        {
            CurrentMode = AdventureInputMode.ChoiceSelection;
            SelectedSkillHandle = GameplayAbilitySpecHandle.Invalid;
        }

        public void EnterSkillTargetSelection(GameplayAbilitySpecHandle handle)
        {
            CurrentMode = AdventureInputMode.SkillTargetSelection;
            SelectedSkillHandle = handle;
        }

        public void Clear()
        {
            CurrentMode = AdventureInputMode.None;
            SelectedSkillHandle = GameplayAbilitySpecHandle.Invalid;
        }
    }

    // Role:
    // Owns Adventure card instances by CardId.
    // It does not know board placement, encounter offers, or view models.
    public sealed class AdventureCards
    {
        private readonly CardRegistry _registry;
        private readonly CardFactory _factory;

        public AdventureCards(CardRegistry registry, CardFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public CardActor Create(CardModelBase model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            ECardFace face = model.Front != null ? ECardFace.Front : ECardFace.Back;
            CardActor card = _factory.Create(_registry.CreateCardId(), model, face);
            _registry.Add(card);
            return card;
        }

        public bool TryGet(uint cardId, out CardActor card)
        {
            return _registry.TryGet(cardId, out card);
        }

        public void Remove(uint cardId)
        {
            _registry.Remove(cardId);
        }

        public void Clear()
        {
            _registry.Clear();
        }
    }

    // Role:
    // Stores Adventure board card placement by CardId.
    // It does not own cards or encounter semantics.
    public sealed class AdventureBoard
    {
        private readonly CardBoardState _state;

        public AdventureBoard(CardBoardState state)
        {
            _state = state;
        }

        public void PlaceCard(ECardZone zone, uint cardId)
        {
            RemoveCardFromAllZones(cardId);
            _state.GetMutableCardIds(zone).Add(cardId);
        }

        public void MoveAllExcept(ECardZone zone, uint keepCardId, ECardZone targetZone)
        {
            List<uint> source = _state.GetMutableCardIds(zone);
            List<uint> target = _state.GetMutableCardIds(targetZone);

            for (int i = source.Count - 1; i >= 0; i--)
            {
                uint cardId = source[i];
                if (cardId == keepCardId)
                    continue;

                source.RemoveAt(i);
                target.Add(cardId);
            }
        }

        public IReadOnlyList<uint> GetCardIds(ECardZone zone)
        {
            return _state.GetCardIds(zone);
        }

        public void RemoveCard(ECardZone zone, uint cardId)
        {
            _state.GetMutableCardIds(zone).Remove(cardId);
        }

        public void ClearZone(ECardZone zone)
        {
            _state.GetMutableCardIds(zone).Clear();
        }

        public void Clear()
        {
            foreach (List<uint> cardIds in _state.GetAllZones())
            {
                cardIds.Clear();
            }
        }

        private void RemoveCardFromAllZones(uint cardId)
        {
            foreach (List<uint> cardIds in _state.GetAllZones())
            {
                cardIds.Remove(cardId);
            }
        }
    }

    // Role:
    // Stores the selected character and its Adventure player card.
    // It does not create cards or activate abilities.
    public sealed class AdventurePlayer
    {
        public CharacterModel Character { get; private set; }
        public CardActor PlayerCard { get; private set; }

        public void Initialize(CharacterModel character, CardActor playerCard)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            PlayerCard = playerCard ?? throw new ArgumentNullException(nameof(playerCard));
        }

        public void Clear()
        {
            Character = null;
            PlayerCard = null;
        }
    }

    // Role:
    // Stores current-stage encounter offers and their offer card bindings.
    // It does not own board placement or card instances.
    public sealed class AdventureStageRuntime
    {
        private readonly List<AdventureStageOfferBinding> _bindings = new();
        private readonly Dictionary<uint, AdventureStageOfferBinding> _bindingsByOfferCardId = new();

        public AdventureStageStartMode StartMode { get; private set; } =
            AdventureStageStartMode.Choice;

        public AdventureStageOfferBinding SelectedBinding { get; private set; }
        public IReadOnlyList<AdventureStageOfferBinding> Bindings => _bindings;

        public void SetStartMode(AdventureStageStartMode startMode)
        {
            StartMode = startMode;
        }

        public void BindOfferCard(AdventureEncounterOffer offer, uint offerCardId)
        {
            AdventureStageOfferBinding binding = new(offer, offerCardId);
            _bindings.Add(binding);
            _bindingsByOfferCardId[offerCardId] = binding;
        }

        public bool TryGetBindingByOfferCardId(uint offerCardId, out AdventureStageOfferBinding binding)
        {
            return _bindingsByOfferCardId.TryGetValue(offerCardId, out binding);
        }

        public void SelectBinding(AdventureStageOfferBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            SelectedBinding = binding;
        }

        public void Clear()
        {
            StartMode = AdventureStageStartMode.Choice;
            _bindings.Clear();
            _bindingsByOfferCardId.Clear();
            SelectedBinding = null;
        }
    }

    // Role:
    // Describes one current-stage encounter candidate.
    // PreviewCard is presentation-only; EncounterCard is the actual created card model.
    public sealed class AdventureEncounterOffer
    {
        public AdventureEncounterOffer(
            uint offerId,
            AdventureEncounterType encounterType,
            CardModelBase previewCard,
            CardModelBase encounterCard)
        {
            OfferId = offerId;
            EncounterType = encounterType;
            PreviewCard = previewCard;
            EncounterCard = encounterCard ?? throw new ArgumentNullException(nameof(encounterCard));
        }

        public uint OfferId { get; }
        public AdventureEncounterType EncounterType { get; }
        public CardModelBase PreviewCard { get; }
        public CardModelBase EncounterCard { get; }
    }

    // Role:
    // Connects a current-stage offer to its actual offer CardId.
    // The card is shown as a choice in presentation but already owns its encounter model.
    public sealed class AdventureStageOfferBinding
    {
        public AdventureStageOfferBinding(AdventureEncounterOffer offer, uint offerCardId)
        {
            Offer = offer ?? throw new ArgumentNullException(nameof(offer));
            OfferCardId = offerCardId;
        }

        public AdventureEncounterOffer Offer { get; }
        public uint OfferCardId { get; }
    }

    // Role:
    // Stores the built encounter sequence draw order for the current Adventure scene.
    // It does not create cards or change board placement.
    public sealed class AdventureEncounterSequenceRuntime
    {
        private readonly List<AdventureEncounterSequenceEntry> _sequence = new();
        private int _drawIndex;

        public void Initialize(AdventureRegionModel region, uint seed)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            _sequence.Clear();
            _drawIndex = 0;

            AdventureEncounterDeckDefinition definition = region.EncounterDeck;
            if (definition?.IsConfigured != true)
                throw new InvalidOperationException($"{region.Name} has no encounter sequence definition.");

            AddDefinitionEntries(definition.StartEntries);

            List<AdventureEncounterSequenceEntry> shuffleEntries = new();
            AddDefinitionEntries(definition.ShuffleEntries, shuffleEntries);

            UnityRandom random = new(RandomUtility.CombineSeed(seed, "AdventureEncounterDeck"));
            for (int i = shuffleEntries.Count - 1; i > 0; i--)
            {
                int j = random.NextInt(0, i + 1);
                (shuffleEntries[i], shuffleEntries[j]) = (shuffleEntries[j], shuffleEntries[i]);
            }

            _sequence.AddRange(shuffleEntries);

            if (definition.BossEntry?.IsValid == true)
                _sequence.Add(CreateEntry(definition.BossEntry));
        }

        private void AddDefinitionEntries(IReadOnlyList<AdventureEncounterDefinition> definitions)
        {
            AddDefinitionEntries(definitions, _sequence);
        }

        private static void AddDefinitionEntries(
            IReadOnlyList<AdventureEncounterDefinition> definitions,
            List<AdventureEncounterSequenceEntry> target)
        {
            for (int i = 0; i < definitions.Count; i++)
            {
                AdventureEncounterDefinition definition = definitions[i];
                if (definition?.IsValid != true)
                    continue;

                for (uint count = 0; count < definition.Count; count++)
                {
                    target.Add(CreateEntry(definition));
                }
            }
        }

        private static AdventureEncounterSequenceEntry CreateEntry(AdventureEncounterDefinition definition)
        {
            return new AdventureEncounterSequenceEntry(
                definition.Type,
                definition.DisplayCard,
                definition.EncounterCard);
        }

        public IReadOnlyList<AdventureEncounterSequenceEntry> Draw(int count)
        {
            int drawCount = Math.Min(count, _sequence.Count - _drawIndex);
            List<AdventureEncounterSequenceEntry> result = new(drawCount);

            for (int i = 0; i < drawCount; i++)
            {
                result.Add(_sequence[_drawIndex]);
                _drawIndex++;
            }

            return result;
        }

        public void Clear()
        {
            _sequence.Clear();
            _drawIndex = 0;
        }
    }

    // Role:
    // Describes one encounter sequence draw instruction.
    public sealed class AdventureEncounterSequenceEntry
    {
        public AdventureEncounterSequenceEntry(
            AdventureEncounterType encounterType,
            CardModelBase previewCard,
            CardModelBase encounterCard)
        {
            EncounterType = encounterType;
            PreviewCard = previewCard;
            EncounterCard = encounterCard;
        }

        public AdventureEncounterType EncounterType { get; }
        public CardModelBase PreviewCard { get; }
        public CardModelBase EncounterCard { get; }
    }

    // Role:
    // Assigns increasing offer ids for current-stage encounter candidates.
    // It does not create cards or mutate stage state.
    public sealed class AdventureOfferFactory
    {
        private uint _nextOfferId = 1;

        public AdventureEncounterOffer Create(
            AdventureEncounterSequenceEntry entry,
            CardModelBase encounterCard)
        {
            CardModelBase previewCard = entry.PreviewCard ?? encounterCard;
            return new AdventureEncounterOffer(
                _nextOfferId++,
                entry.EncounterType,
                previewCard,
                encounterCard);
        }

        public void Clear()
        {
            _nextOfferId = 1;
        }
    }

    // Role:
    // Stores combat participation by CardId for the current Adventure encounter.
    // It does not own cards, execute abilities, subscribe to messages, or publish events.
    public sealed class AdventureCombatRuntime
    {
        private readonly List<uint> _playerCardIds = new();
        private readonly List<uint> _enemyCardIds = new();
        private readonly Dictionary<uint, ECombatSide> _sideByCardId = new();
        private readonly HashSet<uint> _resolvedDeathCardIds = new();

        public ECombatSide CurrentSide { get; private set; }
        public int RoundNumber { get; private set; }
        public bool IsEnded { get; private set; }
        public IReadOnlyList<uint> PlayerCardIds => _playerCardIds;
        public IReadOnlyList<uint> EnemyCardIds => _enemyCardIds;

        public void InitializeCombat(uint playerCardId, uint enemyCardId)
        {
            Clear();
            AddCard(playerCardId, ECombatSide.Player);
            AddCard(enemyCardId, ECombatSide.Enemy);
            CurrentSide = ECombatSide.Player;
            RoundNumber = 1;
            IsEnded = false;
        }

        public void StartEnemyTurn()
        {
            CurrentSide = ECombatSide.Enemy;
        }

        public void StartPlayerTurn()
        {
            CurrentSide = ECombatSide.Player;
            RoundNumber++;
        }

        public void EndCombat()
        {
            IsEnded = true;
        }

        public bool TryGetSide(uint cardId, out ECombatSide side)
        {
            return _sideByCardId.TryGetValue(cardId, out side);
        }

        public bool MarkDeathResolved(uint cardId)
        {
            return _resolvedDeathCardIds.Add(cardId);
        }

        public void Clear()
        {
            _playerCardIds.Clear();
            _enemyCardIds.Clear();
            _sideByCardId.Clear();
            _resolvedDeathCardIds.Clear();
            CurrentSide = ECombatSide.Player;
            RoundNumber = 0;
            IsEnded = false;
        }

        private void AddCard(uint cardId, ECombatSide side)
        {
            _sideByCardId.Add(cardId, side);

            if (side == ECombatSide.Player)
                _playerCardIds.Add(cardId);
            else
                _enemyCardIds.Add(cardId);
        }
    }

    // Role:
    // Maps UI avatar objects used by AbilitySystem cues back to Adventure CardIds.
    // Combat logic uses CardIds while cue handling requires view avatars.
    public sealed class AdventureCardAvatarRegistry
    {
        private readonly Dictionary<object, uint> _cardIdByAvatar = new();

        public void Register(uint cardId, object avatar)
        {
            if (avatar == null)
                return;

            _cardIdByAvatar[avatar] = cardId;
        }

        public void Unregister(object avatar)
        {
            if (avatar == null)
                return;

            _cardIdByAvatar.Remove(avatar);
        }

        public bool TryGetCardId(object avatar, out uint cardId)
        {
            if (avatar == null)
            {
                cardId = default;
                return false;
            }

            return _cardIdByAvatar.TryGetValue(avatar, out cardId);
        }

        public void Clear()
        {
            _cardIdByAvatar.Clear();
        }
    }
}
