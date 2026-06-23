using Domains.Combat.Intent.Data;

namespace Domains.Combat.Intent
{
    public readonly struct IntentDisplayData
    {
        public IntentDisplayData(
            IntentDisplayModel displayModel,
            int numberValue,
            int countValue)
        {
            DisplayModel = displayModel;
            NumberValue = numberValue;
            CountValue = countValue;
        }

        public IntentDisplayModel DisplayModel { get; }
        public int NumberValue { get; }
        public int CountValue { get; }
    }
}
