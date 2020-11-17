namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public sealed class SequencerUniqueEvent : MTrkEvent
    {
        public const byte status = 0x7f;
        public byte[] data;
    }
}
