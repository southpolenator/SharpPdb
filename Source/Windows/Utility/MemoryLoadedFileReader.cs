using SharpUtilities;
using System.Text;

namespace SharpPdb.Windows.Utility
{
    /// <summary>
    /// Class that implements <see cref="IBinaryReader"/> for <see cref="MemoryLoadedFile"/> as stream.
    /// </summary>
    internal unsafe class MemoryLoadedFileReader : IBinaryReader
    {
        /// <summary>
        /// Current position in the memory file.
        /// </summary>
        private byte* pointer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryLoadedFileReader"/> class.
        /// </summary>
        /// <param name="file">The <see cref="MemoryLoadedFile"/> as stream.</param>
        public MemoryLoadedFileReader(MemoryLoadedFile file)
        {
            File = file;
            Position = 0;
        }

        /// <summary>
        /// Gets the <see cref="MemoryLoadedFile"/>.
        /// </summary>
        public MemoryLoadedFile File { get; private set; }

        /// <summary>
        /// Gets or sets the position in the stream.
        /// </summary>
        public long Position
        {
            get => pointer - File.BasePointer;
            set => pointer = File.BasePointer + value;
        }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long Length => File.Length;

        /// <summary>
        /// Gets the remaining number of bytes in the stream.
        /// </summary>
        public long BytesRemaining => Length - Position;

        /// <summary>
        /// Creates duplicate of this stream.
        /// </summary>
        public IBinaryReader Duplicate()
        {
            return new MemoryLoadedFileReader(File)
            {
                Position = Position,
            };
        }

        /// <summary>
        /// Reads <c>byte</c> from the stream.
        /// </summary>
        public byte ReadByte()
        {
            byte value = *pointer;

            pointer++;
            return value;
        }

        /// <summary>
        /// Reads <c>short</c> from the stream.
        /// </summary>
        public short ReadShort()
        {
            short value = *((short*)pointer);

            pointer += 2;
            return value;
        }

        /// <summary>
        /// Reads <c>ushort</c> from the stream.
        /// </summary>
        public ushort ReadUshort()
        {
            ushort value = *((ushort*)pointer);

            pointer += 2;
            return value;
        }

        /// <summary>
        /// Reads <c>int</c> from the stream.
        /// </summary>
        public int ReadInt()
        {
            int value = *((int*)pointer);

            pointer += 4;
            return value;
        }

        /// <summary>
        /// Reads <c>uint</c> from the stream.
        /// </summary>
        public uint ReadUint()
        {
            uint value = *((uint*)pointer);

            pointer += 4;
            return value;
        }

        /// <summary>
        /// Reads <c>long</c> from the stream.
        /// </summary>
        public long ReadLong()
        {
            long value = *((long*)pointer);

            pointer += 8;
            return value;
        }

        /// <summary>
        /// Reads <c>ulong</c> from the stream.
        /// </summary>
        public ulong ReadUlong()
        {
            ulong value = *((ulong*)pointer);

            pointer += 8;
            return value;
        }

        /// <summary>
        /// Reads C-style string (null terminated) from the stream.
        /// </summary>
        public string ReadCString()
        {
            byte* start = pointer;
            byte* end = start;

            while (*end != 0)
                end++;
            pointer = end + 1;
            return new string((sbyte*)start, 0, (int)(end - start));
        }

        /// <summary>
        /// Reads C-style wide (2 bytes) string (null terminated) from the stream.
        /// </summary>
        public string ReadCStringWide()
        {
            ushort* start = (ushort*)pointer;
            ushort* end = start;

            while (*end != 0)
                end++;
            pointer = (byte*)(end + 1);
            return Encoding.Unicode.GetString((byte*)start, (int)((byte*)end - (byte*)start));
        }

        /// <summary>
        /// Moves position by the specified bytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to move the stream.</param>
        public void ReadFake(uint bytes)
        {
            pointer += bytes;
        }
    }
}
