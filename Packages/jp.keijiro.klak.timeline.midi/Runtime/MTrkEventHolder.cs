namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public class MTrkEventHolder<T> where T : MTrkEvent
    {
        public int index;
        public T mtrkEvent;
    }
}
