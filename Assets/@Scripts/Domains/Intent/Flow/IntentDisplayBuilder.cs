using System;
using System.Collections.Generic;
using Domains.Intent.Data;
using Domains.Intent.Runtime;
using UnityEngine;

namespace Domains.Intent.Flow
{
    // Role:
    // Builds cached intent display data from a resolved monster action.
    public sealed class IntentDisplayBuilder
    {
        private static readonly IReadOnlyList<IntentDisplayData> EmptyDisplays =
            Array.Empty<IntentDisplayData>();

        private readonly List<IntentDisplayData> _buffer = new();

        public IReadOnlyList<IntentDisplayData> Build(MonsterActionModel actionModel)
        {
            _buffer.Clear();

            IReadOnlyList<IntentDisplayDefinition> definitions = actionModel.IntentDisplays;
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

                _buffer.Add(
                    new IntentDisplayData(
                        displayModel,
                        number.NumberValue,
                        number.CountValue));
            }

            return _buffer.Count == 0
                ? EmptyDisplays
                : _buffer.ToArray();
        }

        private static IntentNumberData BuildRequiredNumber(
            MonsterActionModel actionModel,
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
