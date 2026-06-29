using System;
using System.Collections.Generic;
using Game.Data;

namespace Domains.View.Widgets
{
    // Role:
    // Provides preloaded choice-card UI rows for the Adventure screen lifetime.
    public sealed class AdventureChoiceCardUIModels
    {
        private readonly Dictionary<EChoiceCardType, AdventureChoiceCardUIModel> _models;

        public AdventureChoiceCardUIModels(
            IReadOnlyDictionary<EChoiceCardType, AdventureChoiceCardUIModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            _models = new Dictionary<EChoiceCardType, AdventureChoiceCardUIModel>(models);
        }

        public AdventureChoiceCardUIModel Get(EChoiceCardType choiceType)
        {
            if (_models.TryGetValue(choiceType, out AdventureChoiceCardUIModel model) &&
                model != null)
            {
                return model;
            }

            throw new InvalidOperationException(
                $"{nameof(AdventureChoiceCardUIModel)} is missing: {choiceType}");
        }
    }
}
