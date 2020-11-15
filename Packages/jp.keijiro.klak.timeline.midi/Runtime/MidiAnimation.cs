using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Klak.Timeline.Midi
{
    // Runtime playable class that calculates MIDI based animation
    [System.Serializable]
    public sealed class MidiAnimation : PlayableBehaviour
    {
        #region Serialized variables

        public string trackName;
        public float tempo = 120;
        public uint duration;
        public uint ticksPerQuarterNote = 96;
        public int eventCount;
        public MTrkEventHolder<MidiEvent>[] midiEvents;
        public MTrkEventHolder<MTrkEvent>[] ignoreEvents;

        MidiTrack _track;
        MidiTrack track
        {
            get
            {
                if (_track != null)
                    return _track;
                var mtrkEvents = new List<MTrkEvent>();
                return _track = new MidiTrack()
                {
                    name = trackName,
                    tempo = tempo,
                    duration = duration,
                    ticksPerQuarterNote = ticksPerQuarterNote,
                    events = Translate(),
                    // events = lyricEvents.Cast<MTrkEvent>().ToList(),
                };
            }
        }

        List<MTrkEvent> Translate()
        {
            var list = new List<MTrkEvent>();
            for (var i = 0; i < eventCount; i++)
            {
                MTrkEvent e = null;
                e = e ?? midiEvents.FirstOrDefault(x => x.index == i)?.Event;
                e = e ?? ignoreEvents.FirstOrDefault(x => x.index == i).Event;
                list.Add(e);
            }
            return list;
        }

        MidiTrackPlayer _player;
        MidiTrackPlayer player
        {
            get
            {
                if (_player != null)
                    return _player;
                return _player = new MidiTrackPlayer(track);
            }
        }
        List<MTrkEvent> mtrkEvents => track.events;

        #endregion

        #region Public properties and methods

        public float DurationInSecond => track.DurationInSecond;

        public float GetValue(Playable playable, MidiControl control)
        {
            if (mtrkEvents == null) return 0;
            var t = (float)playable.GetTime() % DurationInSecond;
            if (control.mode == MidiControl.Mode.NoteEnvelope)
                return GetNoteEnvelopeValue(control, t);
            else if (control.mode == MidiControl.Mode.NoteCurve)
                return GetNoteCurveValue(control, t);
            else // CC
                return GetCCValue(control, t);
        }

        #endregion

        #region PlayableBehaviour implementation

        float previousTime;

        public override void OnGraphStart(Playable playable)
        {
            previousTime = (float)playable.GetTime();
            player.ResetHead(previousTime);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // When the playable is being finished, signals laying in the rest
            // of the clip should be all triggered.
            if (!playable.IsDone()) return;
            var pushAction = GetPushAction(playable, info);
            player.Play((float)playable.GetDuration(), pushAction);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            var pushAction = GetPushAction(playable, info);
            var currentTime = (float)playable.GetTime();
            if (info.evaluationType == FrameData.EvaluationType.Playback)
                player.Play(currentTime, pushAction);
            else
            {
                // // Maximum allowable time difference for scrubbing
                // const float maxDiff = 0.1f;

                // // If the time is increasing and the difference is smaller
                // // than maxDiff, it's being scrubbed.
                // if (currentTime - previousTime < maxDiff)
                // {
                //     // Trigger the signals as usual.
                //     player.Play(currentTime, pushAction);
                // }
                // else
                // {
                //     // It's jumping not scrubbed, so trigger signals laying
                //     // around the current frame.
                //     // var t0 = Mathf.Max(0, current - maxDiff);
                //     // TriggerSignals(t0, current, pushAction);
                // }
            }
            previousTime = currentTime;

            // // Playback or scrubbing?
            // if (info.evaluationType == FrameData.EvaluationType.Playback)
            // {
            //     // Trigger signals between the prrevious/current time.
            //     TriggerSignals(previousTime, current, pushAction);
            // }
            // else
            // {
            //     // Maximum allowable time difference for scrubbing
            //     const float maxDiff = 0.1f;

            //     // If the time is increasing and the difference is smaller
            //     // than maxDiff, it's being scrubbed.
            //     if (current - previousTime < maxDiff)
            //     {
            //         // Trigger the signals as usual.
            //         TriggerSignals(previousTime, current, pushAction);
            //     }
            //     else
            //     {
            //         // It's jumping not scrubbed, so trigger signals laying
            //         // around the current frame.
            //         var t0 = Mathf.Max(0, current - maxDiff);
            //         TriggerSignals(t0, current, pushAction);
            //     }
            // }

            // previousTime = current;
        }

        #endregion

        #region MIDI signal emission

        MidiSignalPool _signalPool = new MidiSignalPool();

        Action<MTrkEvent> GetPushAction(Playable playable, FrameData info)
        {
            return e =>
                info.output.PushNotification(playable, _signalPool.Allocate(e));
        }

        #endregion

        #region Private variables and methods

        (int i0, int i1) GetCCEventIndexAroundTick(uint tick, int ccNumber)
        {
            var last = -1;
            for (var i = 0; i < mtrkEvents.Count; i++)
            {
                if (mtrkEvents[i] is MidiEvent e)
                {
                    if (!e.IsCC || e.data1 != ccNumber) continue;
                    if (e.time > tick) return (last, i);
                    last = i;
                }
            }
            return (last, last);
        }

        (int iOn, int iOff) GetNoteEventsBeforeTick(uint tick, MidiNoteFilter note)
        {
            var iOn = -1;
            var iOff = -1;
            for (var i = 0; i < mtrkEvents.Count; i++)
            {
                if (mtrkEvents[i] is MidiEvent e)
                {
                    if (e.time > tick) break;
                    if (!note.Check(e)) continue;
                    if (e.IsNoteOn) iOn = i; else iOff = i;
                }
            }
            return (iOn, iOff);
        }
        #endregion

        #region Envelope generator

        float CalculateEnvelope(MidiEnvelope envelope, float onTime, float offTime)
        {
            var attackTime = envelope.AttackTime;
            var attackRate = 1 / attackTime;

            var decayTime = envelope.DecayTime;
            var decayRate = 1 / decayTime;

            var level = -offTime / envelope.ReleaseTime;

            if (onTime < attackTime)
            {
                level += onTime * attackRate;
            }
            else if (onTime < attackTime + decayTime)
            {
                level += 1 - (onTime - attackTime) * decayRate * (1 - envelope.SustainLevel);
            }
            else
            {
                level += envelope.SustainLevel;
            }

            return Mathf.Max(0, level);
        }

        #endregion

        #region Value calculation methods

        float GetNoteEnvelopeValue(MidiControl control, float time)
        {
            var tick = track.ConvertSecondToTicks(time);
            var pair = GetNoteEventsBeforeTick(tick, control.noteFilter);

            if (pair.iOn < 0) return 0;
            var eOn = (MidiEvent)mtrkEvents[pair.iOn]; // Note-on event

            // Note-on time
            var onTime = track.ConvertTicksToSecond(eOn.time);

            // Note-off time
            var offTime = pair.iOff < 0 || pair.iOff < pair.iOn ?
                time : track.ConvertTicksToSecond(mtrkEvents[pair.iOff].time);

            var envelope = CalculateEnvelope(
                control.envelope,
                Mathf.Max(0, offTime - onTime),
                Mathf.Max(0, time - offTime)
            );

            var velocity = eOn.data2 / 127.0f;

            return envelope * velocity;
        }

        float GetNoteCurveValue(MidiControl control, float time)
        {
            var tick = track.ConvertSecondToTicks(time);
            var pair = GetNoteEventsBeforeTick(tick, control.noteFilter);

            if (pair.iOn < 0) return 0;
            var eOn = (MidiEvent)mtrkEvents[pair.iOn]; // Note-on event

            // Note-on time
            var onTime = track.ConvertTicksToSecond(eOn.time);

            var curve = control.curve.Evaluate(Mathf.Max(0, time - onTime));
            var velocity = eOn.data2 / 127.0f;

            return curve * velocity;
        }

        float GetCCValue(MidiControl control, float time)
        {
            var tick = track.ConvertSecondToTicks(time);
            var pair = GetCCEventIndexAroundTick(tick, control.ccNumber);

            if (pair.i0 < 0) return 0;
            if (pair.i1 < 0) return ((MidiEvent)mtrkEvents[pair.i0]).data2 / 127.0f;

            var e0 = (MidiEvent)mtrkEvents[pair.i0];
            var e1 = (MidiEvent)mtrkEvents[pair.i1];

            var t0 = track.ConvertTicksToSecond(e0.time);
            var t1 = track.ConvertTicksToSecond(e1.time);

            var v0 = e0.data2 / 127.0f;
            var v1 = e1.data2 / 127.0f;

            return Mathf.Lerp(v0, v1, Mathf.Clamp01((time - t0) / (t1 - t0)));
        }

        #endregion
    }
}
