using Domains.Intent.Data;

namespace Domains.Intent.Runtime
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
