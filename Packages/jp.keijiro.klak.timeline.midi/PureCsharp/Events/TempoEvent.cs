namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public sealed class TempoEvent : MTrkEvent
    {
        public uint tickTempo;
        public float tempo => 60000000f / tickTempo;
    }
}
