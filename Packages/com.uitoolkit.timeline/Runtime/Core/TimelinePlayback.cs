namespace UIToolkit.Timeline
{
    public readonly struct TimelinePlayback
    {
        public TimelinePlayback(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public bool IsValid => Id != 0;

        public static TimelinePlayback Invalid => default;
    }
}
