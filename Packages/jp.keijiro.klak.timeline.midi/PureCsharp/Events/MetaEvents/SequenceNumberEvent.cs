namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public sealed class SequenceNumberEvent : MTrkEvent
    {
        public const byte status = 0x00;
        public byte[] number = new byte[2];
    }
}
