namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public sealed class TrackNameEvent : MTrkEvent
    {
        public const byte status = 0x03;
        public string name;
    }
}
