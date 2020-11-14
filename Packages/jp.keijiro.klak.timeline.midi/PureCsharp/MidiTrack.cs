namespace Klak.Timeline.Midi
{
    public class MidiTrack
    {
        #region Serialized variables

        public string name = "No Name";
        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public MTrkEvent[] events;
        public float DurationInSecond
            => duration / tempo * 60 / ticksPerQuarterNote;

        public uint ConvertSecondToTicks(float time)
            => (uint)(time * tempo / 60 * ticksPerQuarterNote);

        public float ConvertTicksToSecond(uint tick)
            => tick * 60 / (tempo * ticksPerQuarterNote);

        #endregion
    }
}
