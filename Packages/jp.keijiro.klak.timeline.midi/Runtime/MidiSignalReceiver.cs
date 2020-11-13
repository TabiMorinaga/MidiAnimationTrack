using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Klak.Timeline.Midi
{
    // Receives MIDI signals (MIDI event notifications) from a timeline and
    // invokes assigned events.
    [ExecuteInEditMode]
    public sealed class MidiSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        public MidiNoteFilter noteFilter = new MidiNoteFilter
        {
            note = MidiNote.All,
            octave = MidiOctave.All
        };

        public UnityEvent noteOnEvent = new UnityEvent();
        public UnityEvent noteOffEvent = new UnityEvent();
        public Action<MidiEvent> onFireEvent = null;

        public void OnNotify
            (Playable origin, INotification notification, object context)
        {
            var midiEvent = ((MidiSignal)notification).Event;
            if (onFireEvent != null)
                onFireEvent(midiEvent);
            if (noteFilter.Check(midiEvent, out var noteEvent))
            {
                (noteEvent.IsNoteOn ? noteOnEvent : noteOffEvent).Invoke();
            }
        }
    }
}
