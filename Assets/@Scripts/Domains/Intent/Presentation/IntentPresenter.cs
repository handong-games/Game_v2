using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.Intent.Runtime;

namespace Domains.Intent.Presentation
{
    // Role:
    // Converts cached intent runtime data into UI-ready view models.
    public sealed class IntentPresenter
    {
        private static readonly IReadOnlyList<IntentItemViewModel> EmptyItems =
            Array.Empty<IntentItemViewModel>();

        private readonly AdventureCombatRuntime _combat;
        private readonly IntentRuntime _runtime;

        public IntentPresenter(
            AdventureCombatRuntime combat,
            IntentRuntime runtime)
        {
            _combat = combat;
            _runtime = runtime;
        }

        public IReadOnlyList<MonsterIntentRevealViewModel> CreateRevealSequence()
        {
            List<MonsterIntentRevealViewModel> result = new();
            IReadOnlyList<uint> enemyCardIds = _combat.EnemyCardIds;
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                if (TryCreate(enemyCardIds[i], out MonsterIntentRevealViewModel viewModel))
                    result.Add(viewModel);
            }

            return result;
        }

        public bool TryCreate(
            uint enemyCardId,
            out MonsterIntentRevealViewModel viewModel)
        {
            viewModel = null;

            if (!_runtime.TryGet(enemyCardId, out IntentRuntimeState state))
                return false;

            IReadOnlyList<IntentDisplayData> displays = state.CachedIntentDisplays;
            if (displays.Count == 0)
            {
                viewModel = new MonsterIntentRevealViewModel(enemyCardId, EmptyItems);
                return true;
            }

            List<IntentItemViewModel> items = new(displays.Count);
            for (int i = 0; i < displays.Count; i++)
            {
                items.Add(CreateItem(displays[i]));
            }

            viewModel = new MonsterIntentRevealViewModel(enemyCardId, items);
            return true;
        }

        private static IntentItemViewModel CreateItem(IntentDisplayData data)
        {
            return new IntentItemViewModel(
                data.DisplayModel.Icon,
                data.NumberValue,
                data.CountValue,
                FormatNumber(data.NumberValue, data.CountValue));
        }

        private static string FormatNumber(int numberValue, int countValue)
        {
            if (numberValue == 0 && countValue == 0)
                return string.Empty;

            if (countValue <= 1)
                return numberValue.ToString();

            return $"{numberValue}x{countValue}";
        }
    }
}
