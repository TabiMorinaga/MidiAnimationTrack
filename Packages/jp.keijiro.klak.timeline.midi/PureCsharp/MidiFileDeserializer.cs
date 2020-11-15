using System;
using System.Linq;
using System.Collections.Generic;

namespace Klak.Timeline.Midi
{
    // SMF file deserializer implementation
    public static class MidiFileDeserializer
    {
        #region Public members

        public static MidiTrack[] Load(byte[] data)
        {
            var reader = new MidiDataStreamReader(data);

            // Chunk type
            if (reader.ReadChars(4) != "MThd")
                throw new FormatException("Can't find header chunk.");

            // Chunk length
            if (reader.ReadBEUInt32() != 6u)
                throw new FormatException("Length of header chunk must be 6.");

            // Format (unused)
            reader.Advance(2);

            // Number of tracks
            var trackCount = reader.ReadBEUInt16();

            // Ticks per quarter note
            var tpqn = reader.ReadBEUInt16();
            if ((tpqn & 0x8000u) != 0)
                throw new FormatException("SMPTE time code is not supported.");

            // Tracks
            var tracks = new MidiTrack[trackCount];
            float? tempo = null;
            for (var i = 0; i < trackCount; i++)
                tracks[i] = ReadTrack(reader, tpqn, ref tempo);

            return tracks;
        }

        #endregion

        #region Private members

        static MidiTrack ReadTrack(MidiDataStreamReader reader, uint tpqn, ref float? tempo)
        {
            // Chunk type
            if (reader.ReadChars(4) != "MTrk")
                throw new FormatException("Can't find track chunk.");

            // Chunk length
            var chunkEnd = reader.ReadBEUInt32();
            chunkEnd += reader.Position;

            // MIDI event sequence
            var events = new List<MTrkEvent>();
            var allTicks = 0u;
            var stat = (byte)0;

            while (reader.Position < chunkEnd)
            {
                // Delta time
                var ticks = reader.ReadMultiByteValue();
                allTicks += ticks;

                // Status byte
                if ((reader.PeekByte() & 0x80u) != 0)
                    stat = reader.ReadByte();

                switch (stat)
                {
                    case 0xff:
                        events.Add(ReadMetaEvent(allTicks, ticks, stat, reader));
                        break;
                    case 0xf0:
                        events.Add(ReadSysExEvent(allTicks, ticks, stat, reader));
                        break;
                    default:
                        events.Add(ReadMidiEvent(allTicks, ticks, stat, reader));
                        break;
                }
            }

            // Quantize duration with bars.
            var bars = (allTicks + tpqn * 4 - 1) / (tpqn * 4);
            var trackName = "";
            foreach (var e in events)
            {
                switch (e)
                {
                    case TrackNameEvent trackNameEvent:
                        trackName = trackNameEvent.name;
                        break;
                    case TempoEvent tempoEvent:
                        if (tempo == null)
                            tempo = tempoEvent.tempo;
                        break;
                }
            }
            // Asset instantiation
            return new MidiTrack
            {
                name = trackName,
                tempo = tempo ?? 120f,
                duration = bars * tpqn * 4,
                ticksPerQuarterNote = tpqn,
                events = events,
            };
        }

        #endregion

        static MTrkEvent ReadMetaEvent(uint allTicks, uint ticks, byte stat, MidiDataStreamReader reader)
        {
            var eventType = reader.ReadByte();
            switch (eventType)
            {
                // Track Name
                case 0x03:
                    var name = reader.ReadText();
                    return new TrackNameEvent
                    {
                        time = allTicks,
                        ticks = ticks,
                        name = name,
                    };
                // Lyric
                case 0x05:
                    var text = reader.ReadText();
                    return new LyricEvent
                    {
                        time = allTicks,
                        ticks = ticks,
                        text = text,
                    };
                // Tempo
                case 0x51:
                    var len = reader.ReadByte();
                    var tickTempo = reader.ReadBEUint(len);
                    return new TempoEvent
                    {
                        time = allTicks,
                        ticks = ticks,
                        tickTempo = tickTempo,
                    };
                // Ignore
                default:
                    var length = reader.ReadMultiByteValue();
                    var bytes = reader.ReadBytes(length);
                    return new UnknownEvent
                    {
                        time = allTicks,
                        ticks = ticks,
                        bytes = bytes,
                    };
            }
        }
        static MTrkEvent ReadSysExEvent(uint allTicks, uint ticks, byte stat, MidiDataStreamReader reader)
        {
            var bytes = new List<byte>();
            while (true)
            {
                var data = reader.ReadByte();
                bytes.Add(data);
                if (data == 0xf7u)
                    return new UnknownEvent
                    {
                        time = allTicks,
                        ticks = ticks,
                        bytes = bytes.ToArray(),
                    };
            }
        }
        static MidiEvent ReadMidiEvent(uint allTicks, uint ticks, byte stat, MidiDataStreamReader reader)
        {
            var b1 = reader.ReadByte();
            var b2 = (stat & 0xe0u) == 0xc0u ? (byte)0 : reader.ReadByte();
            return new MidiEvent
            {
                time = allTicks,
                ticks = ticks,
                status = stat,
                data1 = b1,
                data2 = b2
            };
        }
    }
}
