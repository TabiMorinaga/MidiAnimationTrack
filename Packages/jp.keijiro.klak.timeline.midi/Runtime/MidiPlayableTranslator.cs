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
                var midiEvents = new List<MidiEvent>();
                var lyricEvents = new List<LyricEvent>();
                foreach (var e in track.events)
                {
                    switch (e)
                    {
                        case MidiEvent midiEvent:
                            midiEvents.Add(midiEvent);
                            break;
                        case LyricEvent lyricEvent:
                            lyricEvents.Add(lyricEvent);
                            break;
                    }
                }
                anim.template.midiEvents = midiEvents.ToArray();
                anim.template.lyricEvents = lyricEvents.ToArray();
                return anim;
            }).ToArray();
            // Asset instantiation
            var asset = ScriptableObject.CreateInstance<MidiFileAsset>();
            asset.tracks = animations;
            return asset;
        }
    }
}
