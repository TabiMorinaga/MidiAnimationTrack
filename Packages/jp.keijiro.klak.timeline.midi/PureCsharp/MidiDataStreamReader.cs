namespace Klak.Timeline.Midi
{
    // MIDI binary data stream reader
    sealed class MidiDataStreamReader
    {
        #region Internal members

        readonly byte[] _data;

        #endregion

        #region Constructor

        public MidiDataStreamReader(byte[] data)
        {
            _data = data;
        }

        #endregion

        #region Current reading position

        public uint Position { get; private set; }

        public void Advance(uint delta)
        {
            Position += delta;
        }

        #endregion

        #region Reader methods

        public byte PeekByte()
        {
            return _data[Position];
        }

        public byte ReadByte()
        {
            return _data[Position++];
        }

        public string ReadChars(int length, int codepage = 932)
        {
            var bytesData = new byte[length];
            for (var i = 0; i < length; i++)
                bytesData[i] = ReadByte();
            return System.Text.Encoding.GetEncoding(codepage).GetString(bytesData);
        }

        public uint ReadBEUint(byte length)
        {
            var number = 0u;
            for (byte i = 0; i < length; i++)
            {
                number += (uint)ReadByte() << (length - i - 1) * 8;
            }
            return number;
        }

        public uint ReadBEUInt32()
        {
            return ReadBEUint(4);
        }

        public uint ReadBEUInt16()
        {
            return ReadBEUint(2);
        }

        public uint ReadMultiByteValue()
        {
            var v = 0u;
            while (true)
            {
                uint b = ReadByte();
                v += b & 0x7fu;
                if (b < 0x80u) break;
                v <<= 7;
            }
            return v;
        }

        public string ReadText()
        {
            var length = ReadByte();
            return ReadChars(length);
        }

        #endregion
    }
}
