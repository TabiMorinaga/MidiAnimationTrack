namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public sealed class KeyEvent : MTrkEvent
    {
        public const byte status = 0x59;
        public byte sf;
        public bool major;
    }
}
