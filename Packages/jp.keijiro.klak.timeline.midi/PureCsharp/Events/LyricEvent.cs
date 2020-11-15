namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public sealed class LyricEvent : MTrkEvent
    {
        public const byte status = 0x05;
        public string text;
    }
}
