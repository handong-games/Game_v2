namespace Domains.Adventure
{
    public sealed class DamageCueData
    {
        public DamageCueData(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }
    }
}
