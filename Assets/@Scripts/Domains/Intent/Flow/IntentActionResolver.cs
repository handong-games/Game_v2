using System.Collections.Generic;
using Domains.Intent.Data;
using Domains.Intent.Runtime;
using Game.Data;
using UnityEngine;
using CardActor = Domains.Card.Card;

namespace Domains.Intent.Flow
{
    // Role:
    // Resolves which authored monster action should be shown and later executed.
    public sealed class IntentActionResolver
    {
        private IntentOverrideRuleSetModel _overrideRuleSet;

        public void SetOverrideRuleSet(IntentOverrideRuleSetModel overrideRuleSet)
        {
            _overrideRuleSet = overrideRuleSet;
        }

        public bool TryResolve(
            CardActor monster,
            IntentRuntimeState state,
            out ResolvedIntentActionData resolvedAction)
        {
            resolvedAction = default;

            if (monster == null)
                return false;

            if (monster.Model is not MonsterModel monsterModel)
            {
                Debug.LogError($"Intent resolution requires MonsterModel. Card: {monster.CardId}.");
                return false;
            }

            IReadOnlyList<MonsterActionModel> actionSequence = monsterModel.ActionSequence;
            if (actionSequence.Count == 0)
            {
                Debug.LogError($"Monster has no intent action sequence. Monster: {monsterModel.name}.");
                return false;
            }

            int actionIndex = state.CurrentIntentActionIndex % actionSequence.Count;
            MonsterActionModel baseAction = actionSequence[actionIndex];
            if (baseAction == null)
            {
                Debug.LogError(
                    $"Monster intent action sequence has null action. Monster: {monsterModel.name}, Index: {actionIndex}.");
                return false;
            }

            IntentOverrideRule appliedOverrideRule = FindOverrideRule(monster);
            MonsterActionModel finalAction = appliedOverrideRule?.ReplacementAction ?? baseAction;
            if (finalAction == null)
            {
                Debug.LogError($"Intent override selected null replacement action. Monster: {monsterModel.name}.");
                return false;
            }

            resolvedAction = new ResolvedIntentActionData(
                baseAction,
                finalAction,
                appliedOverrideRule);
            return true;
        }

        private IntentOverrideRule FindOverrideRule(CardActor monster)
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
    }
}
