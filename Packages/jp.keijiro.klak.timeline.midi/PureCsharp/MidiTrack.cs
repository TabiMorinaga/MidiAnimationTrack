namespace Klak.Timeline.Midi
{
    public class MidiTrack
    {
        #region Serialized variables

        public string name = "No Name";
        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public MidiEvent[] events;
        public float DurationInSecond
        {
            get { return duration / tempo * 60 / ticksPerQuarterNote; }
        }

        public uint ConvertSecondToTicks(float time)
        {
            return (uint)(time * tempo / 60 * ticksPerQuarterNote);
        }

        public float ConvertTicksToSecond(uint tick)
        {
            return tick * 60 / (tempo * ticksPerQuarterNote);
        }

        #endregion
    }
}
