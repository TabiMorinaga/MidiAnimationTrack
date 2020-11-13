using System.Linq;
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
                anim.template.noteEvents = track.events.Cast<NoteEvent>().ToArray();
                return anim;
            }).ToArray();
            // Asset instantiation
            var asset = ScriptableObject.CreateInstance<MidiFileAsset>();
            asset.tracks = animations;
            return asset;
        }
    }
}
