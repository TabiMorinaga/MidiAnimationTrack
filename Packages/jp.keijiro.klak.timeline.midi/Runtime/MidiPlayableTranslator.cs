using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace Klak.Timeline.Midi
{
    public static class MidiPlayableTranslator
    {
        public static MidiFileAsset Translate(MidiTrack[] tracks)
        {
            var animations = tracks.Select(track =>
            {
                var anim = ScriptableObject.CreateInstance<MidiAnimationAsset>();
                anim.name = anim.template.trackName = track.name;
                anim.template.tempo = track.tempo;
                anim.template.duration = track.duration;
                anim.template.ticksPerQuarterNote = track.ticksPerQuarterNote;
                var midiEvents = new List<MTrkEventHolder<MidiEvent>>();
                var ignoreEvents = new List<MTrkEventHolder<MTrkEvent>>();
                // var lyricEvents = new List<LyricEvent>();
                for (var i = 0; i < track.events.Count; i++)
                {
                    switch (track.events[i])
                    {
                        case MidiEvent midiEvent:
                            midiEvents.Add(new MTrkEventHolder<MidiEvent>(i, midiEvent));
                            break;
                        // case LyricEvent lyricEvent:
                        //     lyricEvents.Add(lyricEvent);
                        //     break;
                        default:
                            ignoreEvents.Add(new MTrkEventHolder<MTrkEvent>(i, track.events[i]));
                            break;
                    }
                }
                anim.template.eventCount = track.events.Count;
                anim.template.midiEvents = midiEvents.ToArray();
                // anim.template.lyricEvents = lyricEvents.ToArray();
                anim.template.ignoreEvents = ignoreEvents.ToArray();
                return anim;
            }).ToArray();
            // Asset instantiation
            var asset = ScriptableObject.CreateInstance<MidiFileAsset>();
            asset.tracks = animations;
            return asset;
        }
    }
}
