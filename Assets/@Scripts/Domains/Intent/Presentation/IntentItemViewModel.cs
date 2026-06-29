using UnityEngine;

namespace Domains.Intent.Presentation
{
    // Role:
    // UI-ready data for one rendered intent item.
    public sealed class IntentItemViewModel
    {
        public IntentItemViewModel(
            Sprite icon,
            int numberValue,
            int countValue,
            string numberText)
        {
            Icon = icon != null
                ? icon
                : throw new System.ArgumentNullException(nameof(icon));
            NumberValue = numberValue;
            CountValue = countValue;
            NumberText = numberText ?? string.Empty;
        }

        public Sprite Icon { get; }
        public int NumberValue { get; }
        public int CountValue { get; }
        public string NumberText { get; }
        public bool HasNumber => !string.IsNullOrEmpty(NumberText);
    }
}
