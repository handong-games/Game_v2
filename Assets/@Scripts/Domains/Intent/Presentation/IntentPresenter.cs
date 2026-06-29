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
        private readonly AdventureCombatRuntime _combat;
        private readonly IntentRuntime _runtime;

        public IntentPresenter(
            AdventureCombatRuntime combat,
            IntentRuntime runtime)
        {
            _combat = combat ?? throw new ArgumentNullException(nameof(combat));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public IReadOnlyList<MonsterIntentRevealViewModel> CreateRevealSequence()
        {
            List<MonsterIntentRevealViewModel> result = new();
            IReadOnlyList<uint> enemyCardIds = _combat.EnemyCardIds;
            for (int i = 0; i < enemyCardIds.Count; i++)
            {
                result.Add(Create(enemyCardIds[i]));
            }

            return result;
        }

        public MonsterIntentRevealViewModel Create(uint enemyCardId)
        {
            if (!_runtime.TryGet(enemyCardId, out IntentRuntimeState state))
                throw new InvalidOperationException(
                    $"Cached intent runtime state is missing. Card: {enemyCardId}.");

            IReadOnlyList<IntentDisplayData> displays = state.CachedIntentDisplays;
            if (displays.Count == 0)
                throw new InvalidOperationException(
                    $"Cached intent display is empty. Card: {enemyCardId}.");

            List<IntentItemViewModel> items = new(displays.Count);
            for (int i = 0; i < displays.Count; i++)
            {
                items.Add(CreateItem(displays[i]));
            }

            return new MonsterIntentRevealViewModel(enemyCardId, items);
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
