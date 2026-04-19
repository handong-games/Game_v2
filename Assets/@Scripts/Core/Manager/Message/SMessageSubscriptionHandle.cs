namespace Core.Message
{
    public class SMessageSubscriptionHandle
    {
        public SMessageSubscriptionHandle(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value > 0;
    }
}