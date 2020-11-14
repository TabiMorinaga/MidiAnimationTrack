namespace Klak.Timeline.Midi
{
    public class TempoEvent : MTrkEvent
    {
        public uint tickTempo;
        public float tempo => 60000000f / tickTempo;
    }
}
