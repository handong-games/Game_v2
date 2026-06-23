namespace Domains.Combat.Intent
{
    public readonly struct IntentNumberData
    {
        public IntentNumberData(int numberValue, int countValue)
        {
            NumberValue = numberValue;
            CountValue = countValue;
        }

        public int NumberValue { get; }
        public int CountValue { get; }
    }
}
