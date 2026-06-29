using System;
using System.Collections.Generic;
using Domains.Intent.Data;
using Domains.Intent.Runtime;

namespace Domains.Intent.Flow
{
    // Role:
    // Builds cached intent display data from a resolved monster action.
    public sealed class IntentDisplayBuilder
    {
        private readonly List<IntentDisplayData> _buffer = new();

        public IReadOnlyList<IntentDisplayData> Build(MonsterActionModel actionModel)
        {
            if (actionModel == null)
                throw new ArgumentNullException(nameof(actionModel));

            _buffer.Clear();

            IReadOnlyList<IntentDisplayDefinition> definitions = actionModel.IntentDisplays;
            if (definitions.Count == 0)
                throw new InvalidOperationException($"Monster action has no intent display definitions. Action: {actionModel.name}.");

            for (int i = 0; i < definitions.Count; i++)
            {
                IntentDisplayDefinition definition = definitions[i];
                if (definition == null)
                    throw new InvalidOperationException(
                        $"Intent display definition is null. Action: {actionModel.name}, Index: {i}.");

                IntentDisplayModel displayModel = definition.DisplayModel;
                if (displayModel == null)
                    throw new InvalidOperationException(
                        $"Intent display model is null. Action: {actionModel.name}, Index: {i}.");

                IntentNumberData number = displayModel.RequiresNumber
                    ? BuildRequiredNumber(actionModel, definition, i)
                    : new IntentNumberData(0, 0);

                _buffer.Add(
                    new IntentDisplayData(
                        displayModel,
                        number.NumberValue,
                        number.CountValue));
            }

            return _buffer.ToArray();
        }

        private static IntentNumberData BuildRequiredNumber(
            MonsterActionModel actionModel,
            IntentDisplayDefinition definition,
            int definitionIndex)
        {
            IntentNumberRuleModel numberRule = definition.NumberRule;
            if (numberRule == null)
                throw new InvalidOperationException(
                    $"Intent display requires number but has no number rule. Action: {actionModel.name}, Index: {definitionIndex}.");

            return numberRule.BuildNumber();
        }
    }
}
