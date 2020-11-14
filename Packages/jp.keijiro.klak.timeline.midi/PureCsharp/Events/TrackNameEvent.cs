namespace Klak.Timeline.Midi
{
    public class TrackNameEvent : MTrkEvent
    {
        const byte status = 0x03;
        public string name;
    }
}
