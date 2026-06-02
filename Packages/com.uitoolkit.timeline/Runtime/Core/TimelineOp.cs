namespace UIToolkit.Timeline
{
    public enum TimelineOp : byte
    {
        Notify = 0
    }

    public enum TimelineValueProperty : byte
    {
        Translate = 0,
        Scale = 1,
        Rotate = 2
    }

    public enum TimelineEasing : byte
    {
        Linear = 0,
        EaseIn = 1,
        EaseOut = 2,
        EaseInOut = 3
    }
}
