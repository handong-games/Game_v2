using System;
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public sealed class CombatCardAbilityEntry
    {
        [SerializeField]
        private CardModelBase _cardModel;

        [SerializeField]
        private AttributeSetDefaultsDefinition[] _attributeSetDefaults;

        [SerializeField]
        private GameplayAbility[] _grantedAbilities;

        public CardModelBase CardModel => _cardModel;
        public IReadOnlyList<AttributeSetDefaultsDefinition> AttributeSetDefaults =>
            _attributeSetDefaults ?? Array.Empty<AttributeSetDefaultsDefinition>();

        public IReadOnlyList<GameplayAbility> GrantedAbilities =>
            _grantedAbilities ?? Array.Empty<GameplayAbility>();

#if UNITY_EDITOR
        public bool ValidateEditor(UnityEngine.Object context, int entryIndex)
        {
            bool isValid = true;

            if (_cardModel == null)
            {
                Debug.LogError(
                    $"CombatCardAbility entry {entryIndex} has no card model.",
                    context);

                isValid = false;
            }

            isValid &= ValidateAttributeSetDefaults(context, entryIndex);
            isValid &= ValidateGrantedAbilities(context, entryIndex);

            return isValid;
        }

        private bool ValidateAttributeSetDefaults(UnityEngine.Object context, int entryIndex)
        {
            bool isValid = true;
            HashSet<Type> setTypes = new();
            IReadOnlyList<AttributeSetDefaultsDefinition> definitions = AttributeSetDefaults;

            for (int i = 0; i < definitions.Count; i++)
            {
                AttributeSetDefaultsDefinition definition = definitions[i];
                if (definition == null)
                {
                    Debug.LogError(
                        $"CombatCardAbility entry {entryIndex} has null attribute set defaults. Index: {i}.",
                        context);

                    isValid = false;
                    continue;
                }

                Type setType = definition.GetAttributeSetType();
                if (setType == null)
                {
                    Debug.LogError(
                        $"CombatCardAbility entry {entryIndex} has attribute set defaults with missing type. Index: {i}.",
                        context);

                    isValid = false;
                    continue;
                }

                if (!typeof(AttributeSet).IsAssignableFrom(setType))
                {
                    Debug.LogError(
                        $"CombatCardAbility entry {entryIndex} has invalid attribute set type. Type: {setType.FullName}.",
                        context);

                    isValid = false;
                    continue;
                }

                if (!setTypes.Add(setType))
                {
                    Debug.LogError(
                        $"CombatCardAbility entry {entryIndex} has duplicate attribute set defaults. Type: {setType.FullName}.",
                        context);

                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidateGrantedAbilities(UnityEngine.Object context, int entryIndex)
        {
            bool isValid = true;
            IReadOnlyList<GameplayAbility> abilities = GrantedAbilities;

            if (abilities.Count == 0)
            {
                Debug.LogError(
                    $"CombatCardAbility entry {entryIndex} has no granted abilities.",
                    context);

                return false;
            }

            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i] != null)
                    continue;

                Debug.LogError(
                    $"CombatCardAbility entry {entryIndex} has null granted ability. Index: {i}.",
                    context);

                isValid = false;
            }

            return isValid;
        }
#endif
    }

    [CreateAssetMenu(menuName = "Game/Data/Combat Card Ability Table")]
    public sealed class CombatCardAbilityTable : ScriptableObject
    {
        [SerializeField]
        private CombatCardAbilityEntry[] _entries;

        private Dictionary<ModelKey, CombatCardAbilityEntry> _entriesByCardModel;

        public CombatCardAbilityEntry Get(CardModelBase cardModel)
        {
            if (cardModel == null)
                throw new ArgumentNullException(nameof(cardModel));

            EnsureMap();

            ModelKey modelKey = cardModel.ModelKey;
            if (!_entriesByCardModel.TryGetValue(modelKey, out CombatCardAbilityEntry entry))
                throw new InvalidOperationException(
                    $"CombatCardAbility not found. CardModel: {cardModel.Name} ({modelKey}).");

            return entry;
        }

        public bool TryGet(CardModelBase cardModel, out CombatCardAbilityEntry entry)
        {
            entry = null;
            if (cardModel == null)
                return false;

            EnsureMap();
            return _entriesByCardModel.TryGetValue(cardModel.ModelKey, out entry);
        }

        private void OnValidate()
        {
            _entriesByCardModel = null;
#if UNITY_EDITOR
            ValidateEditor();
#endif
        }

#if UNITY_EDITOR
        public bool ValidateEditor()
        {
            bool isValid = true;
            HashSet<ModelKey> modelKeys = new();

            if (_entries == null)
                return true;

            for (int i = 0; i < _entries.Length; i++)
            {
                CombatCardAbilityEntry entry = _entries[i];
                if (entry == null)
                    continue;

                isValid &= entry.ValidateEditor(this, i);

                CardModelBase cardModel = entry.CardModel;
                if (cardModel == null)
                    continue;

                ModelKey modelKey = cardModel.ModelKey;
                if (!modelKeys.Add(modelKey))
                {
                    Debug.LogError(
                        $"Duplicate CombatCardAbility entry. CardModel: {cardModel.Name} ({modelKey}).",
                        this);

                    isValid = false;
                }
            }

            return isValid;
        }
#endif

        private void EnsureMap()
        {
            if (_entriesByCardModel != null)
                return;

            _entriesByCardModel = new Dictionary<ModelKey, CombatCardAbilityEntry>();

            if (_entries == null)
                return;

            for (int i = 0; i < _entries.Length; i++)
            {
                CombatCardAbilityEntry entry = _entries[i];
                if (entry == null)
                    continue;

                CardModelBase cardModel = entry.CardModel;
                if (cardModel == null)
                    continue;

                ModelKey modelKey = cardModel.ModelKey;
                _entriesByCardModel.Add(modelKey, entry);
            }
        }
    }
}
