namespace UIToolkit.Timeline
{
    public readonly struct TimelineNotify
    {
        public TimelineNotify(
            TimelinePlayback playback,
            TimelineNotifyDefinition definition)
        {
            Playback = playback;
            Definition = definition;
        }

        public TimelinePlayback Playback { get; }
        public TimelineNotifyDefinition Definition { get; }
    }
}
