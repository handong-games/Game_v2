using System.Collections.Generic;
using System;
using Domains.Intent.Data;
using Domains.Intent.Runtime;
using Game.Data;
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

        public ResolvedIntentActionData Resolve(
            CardActor monster,
            IntentRuntimeState state)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (monster.Model is not MonsterModel monsterModel)
                throw new InvalidOperationException(
                    $"Intent resolution requires MonsterModel. Card: {monster.CardId}.");

            IReadOnlyList<MonsterActionModel> actionSequence = monsterModel.ActionSequence;
            if (actionSequence.Count == 0)
                throw new InvalidOperationException(
                    $"Monster has no intent action sequence. Monster: {monsterModel.name}.");

            int actionIndex = state.CurrentIntentActionIndex % actionSequence.Count;
            MonsterActionModel baseAction = actionSequence[actionIndex];
            if (baseAction == null)
                throw new InvalidOperationException(
                    $"Monster intent action sequence has null action. Monster: {monsterModel.name}, Index: {actionIndex}.");

            IntentOverrideRule appliedOverrideRule = FindOverrideRule(monster);
            MonsterActionModel finalAction = appliedOverrideRule?.ReplacementAction ?? baseAction;
            if (finalAction == null)
                throw new InvalidOperationException(
                    $"Intent override selected null replacement action. Monster: {monsterModel.name}.");

            return new ResolvedIntentActionData(
                baseAction,
                finalAction,
                appliedOverrideRule);
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
                    throw new InvalidOperationException(
                        $"Duplicate matching intent override priority. Monster: {monster.CardId}, Priority: {rule.Priority}.");
                }
            }

            return selectedRule;
        }
    }
}
