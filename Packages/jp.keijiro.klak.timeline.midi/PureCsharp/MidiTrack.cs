using System;

namespace Klak.Timeline.Midi
{
    public sealed class MidiTrack
    {
        #region Serialized variables

        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public MidiEvent[] events;

        #endregion

        #region Public properties and methods

        public float DurationInSecond
        {
            get { return duration / tempo * 60 / ticksPerQuarterNote; }
        }

        #endregion

        #region PlayableBehaviour implementation

        public float PreviousTime { get; set; }

        public void OnStart(float previousTime)
        {
            PreviousTime = previousTime;
        }

        #endregion

        #region MIDI signal emission

        public void TriggerSignals(float previous, float current, Action<MidiEvent> onFire)
        {
            var t0 = ConvertSecondToTicks(previous);
            var t1 = ConvertSecondToTicks(current);

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
                TriggerSignalsTick(t0, 0xffffffffu, onFire);
                t0 = 0;
            }

            // Trigger signals between t0 and t1.
            TriggerSignalsTick(t0, t1, onFire);
        }

        void TriggerSignalsTick(uint previous, uint current, Action<MidiEvent> onFire)
        {
            foreach (var e in events)
            {
                if (e.time >= current) break;
                if (e.time < previous) continue;
                if (!e.IsNote) continue;
                onFire(e);
            }
        }

        #endregion

        #region Private variables and methods

        // (int i0, int i1) GetCCEventIndexAroundTick(uint tick, int ccNumber)
        // {
        //     var last = -1;
        //     for (var i = 0; i < events.Length; i++)
        //     {
        //         ref var e = ref events[i];
        //         if (!e.IsCC || e.data1 != ccNumber) continue;
        //         if (e.time > tick) return (last, i);
        //         last = i;
        //     }
        //     return (last, last);
        // }

        // (int iOn, int iOff) GetNoteEventsBeforeTick(uint tick, MidiNoteFilter note)
        // {
        //     var iOn = -1;
        //     var iOff = -1;
        //     for (var i = 0; i < events.Length; i++)
        //     {
        //         ref var e = ref events[i];
        //         if (e.time > tick) break;
        //         if (!note.Check(e)) continue;
        //         if (e.IsNoteOn) iOn = i; else iOff = i;
        //     }
        //     return (iOn, iOff);
        // }

        public uint ConvertSecondToTicks(float time)
        {
            return (uint)(time * tempo / 60 * ticksPerQuarterNote);
        }

        public float ConvertTicksToSecond(uint tick)
        {
            return tick * 60 / (tempo * ticksPerQuarterNote);
        }

        #endregion

        // #region Envelope generator

        // float CalculateEnvelope(MidiEnvelope envelope, float onTime, float offTime)
        // {
        //     var attackTime = envelope.AttackTime;
        //     var attackRate = 1 / attackTime;

        //     var decayTime = envelope.DecayTime;
        //     var decayRate = 1 / decayTime;

        //     var level = -offTime / envelope.ReleaseTime;

        //     if (onTime < attackTime)
        //     {
        //         level += onTime * attackRate;
        //     }
        //     else if (onTime < attackTime + decayTime)
        //     {
        //         level += 1 - (onTime - attackTime) * decayRate * (1 - envelope.SustainLevel);
        //     }
        //     else
        //     {
        //         level += envelope.SustainLevel;
        //     }

        //     return Mathf.Max(0, level);
        // }

        // #endregion

        // #region Value calculation methods

        // float GetNoteEnvelopeValue(MidiControl control, float time)
        // {
        //     var tick = ConvertSecondToTicks(time);
        //     var pair = GetNoteEventsBeforeTick(tick, control.noteFilter);

        //     if (pair.iOn < 0) return 0;
        //     ref var eOn = ref events[pair.iOn]; // Note-on event

        //     // Note-on time
        //     var onTime = ConvertTicksToSecond(eOn.time);

        //     // Note-off time
        //     var offTime = pair.iOff < 0 || pair.iOff < pair.iOn ?
        //         time : ConvertTicksToSecond(events[pair.iOff].time);

        //     var envelope = CalculateEnvelope(
        //         control.envelope,
        //         Mathf.Max(0, offTime - onTime),
        //         Mathf.Max(0, time - offTime)
        //     );

        //     var velocity = eOn.data2 / 127.0f;

        //     return envelope * velocity;
        // }

        // float GetNoteCurveValue(MidiControl control, float time)
        // {
        //     var tick = ConvertSecondToTicks(time);
        //     var pair = GetNoteEventsBeforeTick(tick, control.noteFilter);

        //     if (pair.iOn < 0) return 0;
        //     ref var eOn = ref events[pair.iOn]; // Note-on event

        //     // Note-on time
        //     var onTime = ConvertTicksToSecond(eOn.time);

        //     var curve = control.curve.Evaluate(Mathf.Max(0, time - onTime));
        //     var velocity = eOn.data2 / 127.0f;

        //     return curve * velocity;
        // }

        // float GetCCValue(MidiControl control, float time)
        // {
        //     var tick = ConvertSecondToTicks(time);
        //     var pair = GetCCEventIndexAroundTick(tick, control.ccNumber);

        //     if (pair.i0 < 0) return 0;
        //     if (pair.i1 < 0) return events[pair.i0].data2 / 127.0f;

        //     ref var e0 = ref events[pair.i0];
        //     ref var e1 = ref events[pair.i1];

        //     var t0 = ConvertTicksToSecond(e0.time);
        //     var t1 = ConvertTicksToSecond(e1.time);

        //     var v0 = e0.data2 / 127.0f;
        //     var v1 = e1.data2 / 127.0f;

        //     return Mathf.Lerp(v0, v1, Mathf.Clamp01((time - t0) / (t1 - t0)));
        // }

        // #endregion
    }
}
