namespace Klak.Timeline.Midi
{
    [System.Serializable]
    public class MidiTrack
    {
        #region Serialized variables

        public string name = "No Name";
        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public NoteEvent[] events;
        public float DurationInSecond
        {
            get { return duration / tempo * 60 / ticksPerQuarterNote; }
        }

        #endregion
    }
}
