using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPdb.Windows.Utility
{
    /// <summary>
    /// Represents <see cref="IBinaryReader"/> on top of the existing stream mapped with array of blocks of the same size.
    /// </summary>
    internal class MappedBlockBinaryReader : IBinaryReader
    {
        /// <summary>
        /// Current position in the stream.
        /// </summary>
        private long position;

        /// <summary>
        /// Index of the block that is currently being read.
        /// </summary>
        private int blockIndex;

        /// <summary>
        /// Number of bytes that still remains in the block that is currently being read.
        /// </summary>
        private uint blockRemaining;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedBlockBinaryReader"/> class.
        /// </summary>
        /// <param name="blocks">Array of block indexes in the parent stream.</param>
        /// <param name="blockSize">Single block size in bytes.</param>
        /// <param name="length">Length of this stream in bytes.</param>
        /// <param name="baseReader">Base stream binary reader.</param>
        public MappedBlockBinaryReader(uint[] blocks, uint blockSize, long length, IBinaryReader baseReader)
        {
            Blocks = blocks;
            BlockSize = blockSize;
            BaseReader = baseReader.Duplicate();
            Length = length;
            Position = 0;
        }

        /// <summary>
        /// Gets the array of block indexes in the parent stream.
        /// </summary>
        public uint[] Blocks { get; private set; }

        /// <summary>
        /// Gets a single block size in bytes.
        /// </summary>
        public uint BlockSize { get; private set; }

        /// <summary>
        /// Gets the base stream binary reader.
        /// </summary>
        public IBinaryReader BaseReader { get; private set; }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long Length { get; private set; }

        /// <summary>
        /// Gets the remaining number of bytes in the stream.
        /// </summary>
        public long BytesRemaining => Length - Position;

        /// <summary>
        /// Gets or sets the position in the stream.
        /// </summary>
        public long Position
        {
            get
            {
                return position;
            }

            set
            {
                if (Blocks.Length > 0)
                {
                    long offset = value;

                    if (offset < 0)
                        offset = 0;
                    else if (offset > Length)
                        offset = Length;
                    long blockIndex = offset / BlockSize;
                    uint blockPosition = (uint)(offset % BlockSize);
                    long readerPosition = Blocks[blockIndex] * (long)BlockSize + blockPosition;
                    BaseReader.Position = readerPosition;
                    position = offset;
                    this.blockIndex = (int)blockIndex;
                    blockRemaining = BlockSize - blockPosition;
                }
                else
                {
                    position = 0;
                    blockRemaining = 0;
                    this.blockIndex = 0;
                }
            }
        }

        /// <summary>
        /// Creates duplicate of this stream.
        /// </summary>
        public IBinaryReader Duplicate()
        {
            return new MappedBlockBinaryReader(Blocks, BlockSize, Length, BaseReader)
            {
                Position = Position,
            };
        }

        /// <summary>
        /// Reads <c>byte</c> from the stream.
        /// </summary>
        public unsafe byte ReadByte()
        {
            if (blockRemaining > 1)
            {
                position++;
                blockRemaining--;
                return BaseReader.ReadByte();
            }
            else
            {
                byte value;

                ReadBytes(&value, 1);
                return value;
            }
        }

        /// <summary>
        /// Reads <c>short</c> from the stream.
        /// </summary>
        public unsafe short ReadShort()
        {
            if (blockRemaining > 2)
            {
                position += 2;
                blockRemaining -= 2;
                return BaseReader.ReadShort();
            }
            else
            {
                short value;

                ReadBytes((byte*)&value, 2);
                return value;
            }
        }

        /// <summary>
        /// Reads <c>ushort</c> from the stream.
        /// </summary>
        public unsafe ushort ReadUshort()
        {
            if (blockRemaining > 2)
            {
                position += 2;
                blockRemaining -= 2;
                return BaseReader.ReadUshort();
            }
            else
            {
                ushort value;

                ReadBytes((byte*)&value, 2);
                return value;
            }
        }

        /// <summary>
        /// Reads <c>int</c> from the stream.
        /// </summary>
        public unsafe int ReadInt()
        {
            if (blockRemaining > 4)
            {
                position += 4;
                blockRemaining -= 4;
                return BaseReader.ReadInt();
            }
            else
            {
                int value;

                ReadBytes((byte*)&value, 4);
                return value;
            }
        }

        /// <summary>
        /// Reads <c>uint</c> from the stream.
        /// </summary>
        public unsafe uint ReadUint()
        {
            if (blockRemaining > 4)
            {
                position += 4;
                blockRemaining -= 4;
                return BaseReader.ReadUint();
            }
            else
            {
                uint value;

                ReadBytes((byte*)&value, 4);
                return value;
            }
        }

        /// <summary>
        /// Reads <c>long</c> from the stream.
        /// </summary>
        public unsafe long ReadLong()
        {
            if (blockRemaining > 8)
            {
                position += 8;
                blockRemaining -= 8;
                return BaseReader.ReadLong();
            }
            else
            {
                long value;

                ReadBytes((byte*)&value, 8);
                return value;
            }
        }

        /// <summary>
        /// Reads <c>ulong</c> from the stream.
        /// </summary>
        public unsafe ulong ReadUlong()
        {
            if (blockRemaining > 8)
            {
                position += 8;
                blockRemaining -= 8;
                return BaseReader.ReadUlong();
            }
            else
            {
                ulong value;

                ReadBytes((byte*)&value, 8);
                return value;
            }
        }

        /// <summary>
        /// Reads C-style string (null terminated) from the stream.
        /// </summary>
        public StringReference ReadCString()
        {
            StringReference value = BaseReader.ReadCString();
            uint size = (uint)value.Buffer.Bytes.Length + 1;

            if (size <= blockRemaining)
            {
                blockRemaining -= size;
                position += size;
                CheckMoveReader();
                return value;
            }

            // Check if we are reading from two consecutive blocks
            if (size < BlockSize * 2 && blockIndex + 1 < Blocks.Length && Blocks[blockIndex] + 1 == Blocks[blockIndex + 1])
            {
                uint secondBlockRead = size - blockRemaining;

                position += size;

                // Seek for next block
                blockIndex++;
                if (blockIndex + 1 == Blocks.Length)
                    blockRemaining = (uint)(Length - position);
                else
                    blockRemaining = BlockSize;
                blockRemaining -= secondBlockRead;
                return value;
            }

            // Rewind and fallback to slow reader (byte per byte)
            BaseReader.Position -= size;

            List<byte> bytes = new List<byte>();
            byte b = ReadByte();

            while (b != 0)
            {
                bytes.Add(b);
                b = ReadByte();
            }

            MemoryBuffer buffer = new MemoryBuffer(bytes.ToArray());
            return new StringReference(buffer, StringReference.Encoding.UTF8);
        }

        /// <summary>
        /// Reads C-style wide (2 bytes) string (null terminated) from the stream.
        /// </summary>
        public StringReference ReadCStringWide()
        {
            StringReference value = BaseReader.ReadCStringWide();
            uint size = (uint)value.Buffer.Bytes.Length + 2;

            if (size <= blockRemaining)
            {
                blockRemaining -= size;
                position += size;
                CheckMoveReader();
                return value;
            }

            // Check if we are reading from two consecutive blocks
            if (size < BlockSize * 2 && blockIndex + 1 < Blocks.Length && Blocks[blockIndex] + 1 == Blocks[blockIndex + 1])
            {
                uint secondBlockRead = size - blockRemaining;

                position += size;

                // Seek for next block
                blockIndex++;
                if (blockIndex + 1 == Blocks.Length)
                    blockRemaining = (uint)(Length - position);
                else
                    blockRemaining = BlockSize;
                blockRemaining -= secondBlockRead;
                return value;
            }

            // Rewind and fallback to slow reader (byte per byte)
            BaseReader.Position -= size;

            List<byte> bytes = new List<byte>();
            byte b1 = ReadByte();
            byte b2 = ReadByte();

            while (b1 != 0 || b2 != 0)
            {
                bytes.Add(b1);
                bytes.Add(b2);
                b1 = ReadByte();
                b2 = ReadByte();
            }

            MemoryBuffer buffer = new MemoryBuffer(bytes.ToArray());
            return new StringReference(buffer, StringReference.Encoding.Unicode);
        }

        /// <summary>
        /// Reads bytes buffer from the stream.
        /// </summary>
        /// <param name="bytes">Buffer pointer where bytes should be stored.</param>
        /// <param name="count">Number of bytes to from the stream</param>
        public unsafe void ReadBytes(byte* bytes, uint count)
        {
            if (count < blockRemaining)
            {
                BaseReader.ReadBytes(bytes, count);
                position += count;
                blockRemaining -= count;
            }
            else
                while (count > 0)
                {
                    uint read = count < blockRemaining ? count : blockRemaining;

                    BaseReader.ReadBytes(bytes, read);
                    position += read;
                    blockRemaining -= read;
                    CheckMoveReader();
                    count -= read;
                    bytes += read;
                }
        }

        /// <summary>
        /// Moves position by the specified bytes.
        /// </summary>
        /// <param name="bytes">Number of bytes to move the stream.</param>
        public void Move(uint bytes)
        {
            while (bytes > blockRemaining)
            {
                bytes -= blockRemaining;
                Move(blockRemaining);
            }

            position += bytes;
            blockRemaining -= bytes;
            BaseReader.Move(bytes);
            CheckMoveReader();
        }

        /// <summary>
        /// Checks if we have encountered end of current block and moves base reader to the position of new block.
        /// </summary>
        private void CheckMoveReader()
        {
            if (blockRemaining == 0)
            {
                // Seek for next block
                blockIndex++;
                if (blockIndex < Blocks.Length)
                {
                    if (blockIndex + 1 == Blocks.Length)
                        blockRemaining = (uint)(Length - position);
                    else
                        blockRemaining = BlockSize;
                    if (Blocks[blockIndex - 1] + 1 != Blocks[blockIndex])
                        BaseReader.Position = Blocks[blockIndex] * (long)BlockSize;
                }
            }
        }

        /// <summary>
        /// Reads memory buffer from the stream.
        /// </summary>
        /// <param name="length">Number of bytes to read from the stream.</param>
        /// <returns>Memory buffer read from the stream.</returns>
        public MemoryBuffer ReadBuffer(uint length)
        {
            if (blockRemaining < length)
            {
                blockRemaining -= length;
                position += length;
                return BaseReader.ReadBuffer(length);
            }
            return new MemoryBuffer(this.ReadByteArray((int)length));
        }
    }
}
