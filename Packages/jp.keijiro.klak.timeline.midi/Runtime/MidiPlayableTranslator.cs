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
                anim.name = track.name;
                anim.template.Initialize(track);
                return anim;
            }).ToArray();
            // Asset instantiation
            var asset = ScriptableObject.CreateInstance<MidiFileAsset>();
            asset.tracks = animations;
            return asset;
        }
    }
}
