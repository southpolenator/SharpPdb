using SharpPdb.Windows.Utility;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpPdb.Windows
{
    /// <summary>
    /// Represents PDB stream hash table. Since we are using it as readonly, it can easily be converted to <see cref="HashTable.Dictionary"/>.
    /// </summary>
    public class HashTable
    {
        /// <summary>
        /// Buckets array. Since it is readonly, every nonempty entry in this array corresponds to present element.
        /// </summary>
        private Tuple<uint, uint>[] buckets;

        /// <summary>
        /// Bit array of present entries.
        /// </summary>
        private uint[] present;

        /// <summary>
        /// Bit array of deleted entries.
        /// </summary>
        private uint[] deleted;

        /// <summary>
        /// Cache for <see cref="Dictionary"/>.
        /// </summary>
        private SimpleCacheStruct<Dictionary<uint, uint>> dictionaryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="HashTable"/> class.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public HashTable(IBinaryReader reader)
        {
            Size = reader.ReadUint();
            Capacity = reader.ReadUint();
            if (Capacity == 0)
                throw new Exception("Invalid Hash Table Capacity");
            if (Size > MaxLoad(Capacity))
                throw new Exception("Invalid Hash Table Size");
            buckets = new Tuple<uint, uint>[Capacity];
            present = ReadSparseBitArray(reader);
            if (CountBits(present) != Size)
                throw new Exception("Present bit vector does not match size!");
            deleted = ReadSparseBitArray(reader);
            if (BitsIntersect(present, deleted))
                throw new Exception("Present bit vector interesects deleted!");
            for (int i = 0, index = 0; i < present.Length; i++)
                for (uint j = 0, bit = 1; j < 32; j++, bit <<= 1, index++)
                    if ((present[i] & bit) != 0)
                        buckets[index] = Tuple.Create(reader.ReadUint(), reader.ReadUint());
            dictionaryCache = SimpleCache.CreateStruct(() =>
            {
                Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();

                foreach (var tuple in buckets)
                    if (tuple != null)
                        dictionary.Add(tuple.Item1, tuple.Item2);
                return dictionary;
            });
        }

        /// <summary>
        /// Gets the number of entries in the dictionary.
        /// </summary>
        public uint Size { get; private set; }

        /// <summary>
        /// Gets the capacity of buckets array of this dictionary.
        /// </summary>
        public uint Capacity { get; private set; }

        /// <summary>
        /// Converts hash table to .NET dictionary.
        /// </summary>
        public Dictionary<uint, uint> Dictionary => dictionaryCache.Value;

        /// <summary>
        /// Computes hash of the specified string.
        /// </summary>
        /// <remarks>Corresponds to <c>Hasher::lhashPbCb`</c> in PDB/include/misc.h. Used for name hash table and TPI/IPI hashes.</remarks>
        /// <param name="s">The string.</param>
        /// <returns>Hash of the string.</returns>
        public static uint HashStringV1(string s)
        {
            return HashV1(System.Text.Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Computes hash of the specified byte array.
        /// </summary>
        /// <remarks>Corresponds to <c>Hasher::lhashPbCb`</c> in PDB/include/misc.h. Used for name hash table and TPI/IPI hashes.</remarks>
        /// <param name="bytes">The byte array.</param>
        /// <returns>Hash of the byte array.</returns>
        public static unsafe uint HashV1(byte[] bytes)
        {
            fixed (byte* b = bytes)
            {
                return HashV1(b, bytes.Length);
            }
        }

        /// <summary>
        /// Computes hash of the specified byte array.
        /// </summary>
        /// <remarks>Corresponds to <c>SigForPbCb</c> in langapi/shared/crc32.h.</remarks>
        /// <param name="bytes">The byte array</param>
        /// <returns>Hash of the byte array.</returns>
        public static uint HashBufferV8(byte[] bytes)
        {
            return Crc32.Update(0, bytes);
        }

        /// <summary>
        /// Computes hash of the specified byte buffer.
        /// </summary>
        /// <remarks>Corresponds to <c>Hasher::lhashPbCb`</c> in PDB/include/misc.h. Used for name hash table and TPI/IPI hashes.</remarks>
        /// <param name="bytes">The byte buffer.</param>
        /// <param name="length">Length of the buffer.</param>
        /// <returns>Hash of the byte buffer.</returns>
        public static unsafe uint HashV1(byte* bytes, int length)
        {
            uint result = 0;
            uint* longs = (uint*)bytes;
            int longsLength = length / 4; // 4 = sizeof(uint);

            for (int i = 0; i < longsLength; i++, longs++)
                result ^= *longs;
            byte* remainder = (byte*)longs;
            int reminderLength = length % 4;

            // Maximum of 3 bytes left. Hash a 2 byte word if possible, then hash the
            // possibly remaining 1 byte.
            if (reminderLength >= 2)
            {
                ushort value = *((ushort*)remainder);
                result ^= value;
                remainder += 2;
                reminderLength -= 2;
            }

            // hash possible odd byte
            if (reminderLength == 1)
                result ^= *(remainder++);

            const uint toLowerMask = 0x20202020;

            result |= toLowerMask;
            result ^= (result >> 11);
            return result ^ (result >> 16);
        }

        /// <summary>
        /// Computes hash of the specified string.
        /// </summary>
        /// <remarks>Corresponds to <c>HasherV2::HashULONG`</c> in PDB/include/misc.h. Used for name hash table.</remarks>
        /// <param name="s">The string.</param>
        /// <returns>Hash of the string.</returns>
        public static uint HashStringV2(string s)
        {
            return HashV2(System.Text.Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Computes hash of the specified byte array.
        /// </summary>
        /// <remarks>Corresponds to <c>HasherV2::HashULONG`</c> in PDB/include/misc.h. Used for name hash table.</remarks>
        /// <param name="bytes">The byte array.</param>
        /// <returns>Hash of the byte array.</returns>
        public static unsafe uint HashV2(byte[] bytes)
        {
            fixed (byte* b = bytes)
            {
                return HashV2(b, bytes.Length);
            }
        }

        /// <summary>
        /// Computes hash of the specified byte buffer.
        /// </summary>
        /// <remarks>Corresponds to <c>HasherV2::HashULONG`</c> in PDB/include/misc.h. Used for name hash table.</remarks>
        /// <param name="bytes">The byte buffer.</param>
        /// <param name="length">Length of the buffer.</param>
        /// <returns>Hash of the byte buffer.</returns>
        public static unsafe uint HashV2(byte* bytes, int length)
        {
            uint hash = 0xb170a1bf;
            uint* longs = (uint*)bytes;
            int longsLength = length / 4; // 4 = sizeof(uint);

            for (int i = 0; i < longsLength; i++, longs++)
            {
                hash += *longs;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            byte* remainder = (byte*)longs;
            int reminderLength = length % 4;

            for (int i = 0; i < reminderLength; i++, remainder++)
            {
                hash += *remainder;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            return hash * 1664525U + 1013904223U;
        }

        /// <summary>
        /// Reads bit array from the specified stream.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        private static uint[] ReadSparseBitArray(IBinaryReader reader)
        {
            uint entries = reader.ReadUint();

            return reader.ReadUintArray((int)entries);
        }

        /// <summary>
        /// Counts number of set bits in bit array.
        /// </summary>
        /// <param name="bitArray">Bit array.</param>
        private static int CountBits(uint[] bitArray)
        {
            int count = 0;

            for (int i = 0; i < bitArray.Length; i++)
                for (uint j = 0, bit = 1; j < 32; j++, bit <<= 1)
                    if ((bitArray[i] & bit) != 0)
                        count++;
            return count;
        }

        /// <summary>
        /// Checks whether two bit arrays intersect.
        /// </summary>
        /// <param name="array1">First bit array.</param>
        /// <param name="array2">Second bit array.</param>
        private static bool BitsIntersect(uint[] array1, uint[] array2)
        {
            int end = Math.Min(array1.Length, array2.Length);

            for (int i = 0; i < end; i++)
                for (uint j = 0, bit = 1; j < 32; j++, bit <<= 1)
                    if ((array1[i] & bit) != 0 && (array2[i] & bit) != 0)
                        return true;
            return false;
        }

        /// <summary>
        /// Calculates maximum number of entries that hash table can have in the buckets array.
        /// </summary>
        /// <param name="capacity">Buckets array size.</param>
        private static uint MaxLoad(uint capacity)
        {
            return capacity * 2 / 3 + 1;
        }
    }
}
