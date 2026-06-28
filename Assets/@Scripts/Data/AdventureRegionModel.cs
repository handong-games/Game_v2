using System;
using System.Collections.Generic;
using Domains.Adventure;
using Game.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    public enum AdventureStageStartMode
    {
        Choice,
        ImmediateEncounter,
    }

    // Role:
    // Defines one Adventure region as static scene-start data.
    // Runtime state, current stage, selected card, and combat progress must live outside this model.
    [CreateAssetMenu(menuName = "Game/Data/Adventure Region")]
    public sealed class AdventureRegionModel : AbstractModel<EAdventure>
    {
        [Header("Entry Presentation")]
        [SerializeField]
        private AdventureEntryPresentationModel _entryPresentation;

        [Header("Region")]
        [SerializeField]
        private LocalizedString _localizedRegionName;

        [SerializeField]
        private uint _startDrawCount = 2;

        [SerializeField]
        private uint _maxStageCount = 1;

        [Header("Stage Definitions")]
        [SerializeField]
        private AdventureStageDefinition[] _stages;

        [Header("Encounter Deck")]
        [SerializeField]
        private AdventureEncounterDeckDefinition _encounterDeck;

        public AdventureEntryPresentationModel EntryPresentation => _entryPresentation;
        public LocalizedString LocalizedRegionName => _localizedRegionName;
        public uint StartDrawCount => _startDrawCount == 0 ? 1 : _startDrawCount;
        public uint MaxStageCount => HasStages ? (uint)_stages.Length : (_maxStageCount == 0 ? 1 : _maxStageCount);
        public IReadOnlyList<AdventureStageDefinition> Stages => _stages ?? Array.Empty<AdventureStageDefinition>();
        public AdventureEncounterDeckDefinition EncounterDeck => _encounterDeck;
        public bool HasStages => _stages != null && _stages.Length > 0;

        public bool TryGetStage(uint stageNumber, out AdventureStageDefinition stage)
        {
            stage = null;

            if (!HasStages || stageNumber == 0)
                return false;

            int index = (int)stageNumber - 1;
            if (index < 0 || index >= _stages.Length)
                return false;

            stage = _stages[index];
            return stage != null;
        }
    }

    // Role:
    // Holds the visual/localized data needed when AdventureScene first appears.
    // It is not stage, deck, combat, or runtime state.
    [Serializable]
    public sealed class AdventureEntryPresentationModel
    {
        [SerializeField]
        private LocalizedString _title;

        [SerializeField]
        private LocalizedString _subtitle;

        [SerializeField]
        private Sprite _background;

        [SerializeField]
        private Sprite _emblem;

        public LocalizedString Title => _title;
        public LocalizedString Subtitle => _subtitle;
        public Sprite Background => _background;
        public Sprite Emblem => _emblem;
    }

    // Role:
    // Defines one stage's static draw rules.
    // Stage order is the array index in AdventureRegionModel, so no stage number is stored here.
    [Serializable]
    public sealed class AdventureStageDefinition
    {
        [SerializeField]
        private AdventureStageStartMode _startMode = AdventureStageStartMode.Choice;

        [SerializeField]
        private uint _drawCount = 2;

        public AdventureStageStartMode StartMode => _startMode;
        public uint DrawCount => _drawCount == 0 ? 1 : _drawCount;
    }

    // Role:
    // Defines the encounter offer deck for a region using direct model references.
    // It replaces scattered count/pool data when configured.
    [Serializable]
    public sealed class AdventureEncounterDeckDefinition
    {
        [SerializeField]
        private AdventureEncounterDefinition[] _startEntries;

        [SerializeField]
        private AdventureEncounterDefinition[] _shuffleEntries;

        [SerializeField]
        private AdventureEncounterDefinition _bossEntry;

        public IReadOnlyList<AdventureEncounterDefinition> StartEntries =>
            _startEntries ?? Array.Empty<AdventureEncounterDefinition>();

        public IReadOnlyList<AdventureEncounterDefinition> ShuffleEntries =>
            _shuffleEntries ?? Array.Empty<AdventureEncounterDefinition>();

        public AdventureEncounterDefinition BossEntry => _bossEntry;

        public bool IsConfigured =>
            HasAny(StartEntries) ||
            HasAny(ShuffleEntries) ||
            _bossEntry?.IsValid == true;

        private static bool HasAny(IReadOnlyList<AdventureEncounterDefinition> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i]?.IsValid == true)
                    return true;
            }

            return false;
        }
    }

    // Role:
    // Defines one encounter card candidate.
    // DisplayCard is what the player sees as an offer; EncounterCard is the actual card used after selection.
    [Serializable]
    public sealed class AdventureEncounterDefinition
    {
        [SerializeField]
        private AdventureEncounterType _type;

        [SerializeField]
        private BasicCardModel _displayCard;

        [SerializeField]
        private CardModelBase _encounterCard;

        [SerializeField]
        private uint _count = 1;

        public AdventureEncounterType Type => _type;
        public BasicCardModel DisplayCard => _displayCard;
        public CardModelBase EncounterCard => _encounterCard;
        public uint Count => _count == 0 ? 1 : _count;
        public bool IsValid => _encounterCard != null;
    }

}
