using System;

namespace Klak.Timeline.Midi
{
    public class MidiTrackPlayer
    {

        #region Parameters

        public MidiTrackPlayer(MidiTrack track)
        {
            this.track = track;
        }
        public MidiTrack track;
        float tempo => track.tempo;
        public uint duration => track.duration;
        uint ticksPerQuarterNote => track.ticksPerQuarterNote;
        MTrkEvent[] events => track.events;
        public float previousTime { get; set; }

        #endregion


        #region MIDI signal emission

        public void TriggerSignals
            (float previous, float current, Action<MTrkEvent> onPushEvent)
        {
            var t0 = track.ConvertSecondToTicks(previous);
            var t1 = track.ConvertSecondToTicks(current);

            // Resolve wrapping-around cases by offsetting.
            if (t1 < t0) t1 += (t0 / duration + 1) * duration;

            // Offset both the points to make t0 < duration.
            var offs = (t0 / duration) * duration;
            t0 -= offs;
            t1 -= offs;

            // Resolve loops.
            for (; t1 >= duration; t1 -= duration)
            {
                // Trigger signals between t0 and the end of the clip.
                TriggerSignalsTick(t0, 0xffffffffu, onPushEvent);
                t0 = 0;
            }

            // Trigger signals between t0 and t1.
            TriggerSignalsTick(t0, t1, onPushEvent);

        }

        void TriggerSignalsTick(uint previous, uint current, Action<MTrkEvent> onPushEvent)
        {
            foreach (var e in events)
            {
                if (e.time >= current) break;
                if (e.time < previous) continue;
                // if (!e.IsNote) continue;
                onPushEvent(e);
            }
        }

        #endregion
    }
}
