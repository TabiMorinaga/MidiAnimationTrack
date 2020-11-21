using System.Collections.Generic;

namespace Midity
{
    public class MidiTrack
    {
        #region Serialized variables

        public string name = "";
        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public List<MTrkEvent> events = new List<MTrkEvent>();
        public float DurationInSecond
            => duration / tempo * 60 / ticksPerQuarterNote;

        public uint ConvertSecondToTicks(float time)
            => (uint)(time * tempo / 60 * ticksPerQuarterNote);

        public float ConvertTicksToSecond(uint tick)
            => tick * 60 / (tempo * ticksPerQuarterNote);

        #endregion
    }
}
