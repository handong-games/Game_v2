using System;
using System.Collections.Generic;
using Domains.Combat;
using Domains.Combat.Intent.Data;
using Game.Data;
using UnityEngine;

namespace Domains.Combat.Intent
{
    public sealed class IntentSystem
    {
        private static readonly IReadOnlyList<IntentDisplayData> EmptyDisplays =
            Array.Empty<IntentDisplayData>();

        private readonly Dictionary<CombatCard, IntentRuntimeState> _states = new();
        private readonly List<IntentDisplayData> _displayBuffer = new();
        private IntentOverrideRuleSetModel _overrideRuleSet;

        public event Action<CombatCard, IReadOnlyList<IntentDisplayData>> IntentChanged;
        public event Action<CombatCard> IntentCleared;

        public void SetOverrideRuleSet(IntentOverrideRuleSetModel overrideRuleSet)
        {
            _overrideRuleSet = overrideRuleSet;
        }

        public void Clear()
        {
            _states.Clear();
        }

        public void Initialize(IEnumerable<CombatCard> monsters)
        {
            Clear();

            if (monsters == null)
                return;

            foreach (CombatCard monster in monsters)
            {
                if (monster == null)
                    continue;

                _states[monster] = new IntentRuntimeState();
            }
        }

        public void RebuildAll(IEnumerable<CombatCard> monsters)
        {
            if (monsters == null)
                return;

            foreach (CombatCard monster in monsters)
            {
                if (monster == null)
                    continue;

                Rebuild(monster);
            }
        }

        public bool Rebuild(CombatCard monster)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            IntentRuntimeState state = GetOrCreateState(monster);
            if (!ResolveAction(monster, state, out ResolvedIntentActionData resolvedAction))
                return false;

            IReadOnlyList<IntentDisplayData> displays = BuildDisplayCache(resolvedAction.FinalActionModel);
            state.SetCache(resolvedAction, displays);
            IntentChanged?.Invoke(monster, displays);
            return true;
        }

        public bool TryGetIntentDisplays(
            CombatCard monster,
            out IReadOnlyList<IntentDisplayData> displays)
        {
            displays = EmptyDisplays;

            if (monster == null)
                return false;

            if (!_states.TryGetValue(monster, out IntentRuntimeState state))
            {
                Debug.LogError($"Intent state not found. Monster: {monster.CardId}.");
                return false;
            }

            displays = state.CachedIntentDisplays;
            return true;
        }

        public bool TryGetResolvedAction(
            CombatCard monster,
            out ResolvedIntentActionData resolvedAction)
        {
            resolvedAction = default;

            if (monster == null)
                return false;

            if (!_states.TryGetValue(monster, out IntentRuntimeState state))
            {
                Debug.LogError($"Intent state not found. Monster: {monster.CardId}.");
                return false;
            }

            if (!state.TryGetCachedResolvedAction(out resolvedAction))
            {
                Debug.LogError($"Resolved intent action not found. Monster: {monster.CardId}.");
                return false;
            }

            return true;
        }

        public bool ConsumeResolvedAction(CombatCard monster)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            if (!_states.TryGetValue(monster, out IntentRuntimeState state))
            {
                Debug.LogError($"Intent state not found. Monster: {monster.CardId}.");
                return false;
            }

            if (!(monster.Card.Model is MonsterModel monsterModel))
            {
                Debug.LogError($"Intent consume requested for non-monster card. Card: {monster.CardId}.");
                return false;
            }

            bool consumed = state.Consume(monsterModel.ActionSequence.Count);
            if (!consumed)
            {
                Debug.LogError($"Resolved intent action not found on consume. Monster: {monster.CardId}.");
                return false;
            }

            IntentCleared?.Invoke(monster);
            return true;
        }

        public void Remove(CombatCard monster)
        {
            if (monster == null)
                return;

            if (_states.Remove(monster))
                IntentCleared?.Invoke(monster);
        }

        private IntentRuntimeState GetOrCreateState(CombatCard monster)
        {
            if (_states.TryGetValue(monster, out IntentRuntimeState state))
                return state;

            Debug.LogError($"Intent state was missing and has been recreated. Monster: {monster.CardId}.");
            state = new IntentRuntimeState();
            _states.Add(monster, state);
            return state;
        }

        private bool ResolveAction(
            CombatCard monster,
            IntentRuntimeState state,
            out ResolvedIntentActionData resolvedAction)
        {
            resolvedAction = default;

            if (!(monster.Card.Model is MonsterModel monsterModel))
            {
                Debug.LogError($"Intent resolution requires MonsterModel. Card: {monster.CardId}.");
                return false;
            }

            IReadOnlyList<IntentActionModel> actionSequence = monsterModel.ActionSequence;
            if (actionSequence.Count == 0)
            {
                Debug.LogError($"Monster has no intent action sequence. Monster: {monsterModel.name}.");
                return false;
            }

            int actionIndex = state.CurrentIntentActionIndex % actionSequence.Count;
            IntentActionModel baseAction = actionSequence[actionIndex];
            if (baseAction == null)
            {
                Debug.LogError(
                    $"Monster intent action sequence has null action. Monster: {monsterModel.name}, Index: {actionIndex}.");
                return false;
            }

            IntentOverrideRule appliedOverrideRule = FindOverrideRule(monster);
            IntentActionModel finalAction = appliedOverrideRule?.ReplacementAction ?? baseAction;
            if (finalAction == null)
            {
                Debug.LogError(
                    $"Intent override selected null replacement action. Monster: {monsterModel.name}.");
                return false;
            }

            resolvedAction = new ResolvedIntentActionData(
                baseAction,
                finalAction,
                appliedOverrideRule);
            return true;
        }

        private IntentOverrideRule FindOverrideRule(CombatCard monster)
        {
            if (_overrideRuleSet == null)
                return null;

            IntentOverrideRule selectedRule = null;
            IReadOnlyList<IntentOverrideRule> rules = _overrideRuleSet.Rules;
            for (int i = 0; i < rules.Count; i++)
            {
                IntentOverrideRule rule = rules[i];
                if (rule == null ||
                    rule.ReplacementAction == null ||
                    !rule.ConditionTag.IsValid ||
                    !monster.AbilitySystem.OwnedTags.HasTagExact(rule.ConditionTag))
                {
                    continue;
                }

                if (selectedRule == null || rule.Priority > selectedRule.Priority)
                {
                    selectedRule = rule;
                    continue;
                }

                if (rule.Priority == selectedRule.Priority)
                {
                    Debug.LogError(
                        $"Duplicate matching intent override priority. Monster: {monster.CardId}, Priority: {rule.Priority}.");
                }
            }

            return selectedRule;
        }

        private IReadOnlyList<IntentDisplayData> BuildDisplayCache(IntentActionModel actionModel)
        {
            _displayBuffer.Clear();

            IReadOnlyList<IntentDisplayDefinition> definitions = actionModel.DisplayDefinitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                IntentDisplayDefinition definition = definitions[i];
                if (definition == null)
                {
                    Debug.LogError($"Intent display definition is null. Action: {actionModel.name}, Index: {i}.");
                    continue;
                }

                IntentDisplayModel displayModel = definition.DisplayModel;
                if (displayModel == null)
                {
                    Debug.LogError($"Intent display model is null. Action: {actionModel.name}, Index: {i}.");
                    continue;
                }

                IntentNumberData number = displayModel.RequiresNumber
                    ? BuildRequiredNumber(actionModel, definition, i)
                    : new IntentNumberData(0, 0);

                _displayBuffer.Add(
                    new IntentDisplayData(
                        displayModel,
                        number.NumberValue,
                        number.CountValue));
            }

            return _displayBuffer.Count == 0
                ? EmptyDisplays
                : _displayBuffer.ToArray();
        }

        private static IntentNumberData BuildRequiredNumber(
            IntentActionModel actionModel,
            IntentDisplayDefinition definition,
            int definitionIndex)
        {
            IntentNumberRuleModel numberRule = definition.NumberRule;
            if (numberRule == null)
            {
                Debug.LogError(
                    $"Intent display requires number but has no number rule. Action: {actionModel.name}, Index: {definitionIndex}.");
                return new IntentNumberData(0, 1);
            }

            return numberRule.BuildNumber();
        }
    }
}
