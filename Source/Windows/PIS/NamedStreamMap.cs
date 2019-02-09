using SharpPdb.Windows.Utility;
using SharpUtilities;
using System.Collections.Generic;

namespace SharpPdb.Windows.PIS
{
    /// <summary>
    /// Map between stream name and stream id.
    /// </summary>
    public class NamedStreamMap
    {
        /// <summary>
        /// Cache for <see cref="Streams"/>.
        /// </summary>
        private SimpleCacheStruct<Dictionary<string, int>> streamsCache;

        /// <summary>
        /// Cache for <see cref="StreamsUppercase"/>.
        /// </summary>
        private SimpleCacheStruct<Dictionary<string, int>> streamsUppercaseCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedStreamMap"/> class.
        /// </summary>
        /// <param name="reader">Stream binary reader.</param>
        public NamedStreamMap(IBinaryReader reader)
        {
            uint stringsSizeInBytes = reader.ReadUint();
            StringsStream = reader.ReadSubstream(stringsSizeInBytes);
            HashTable = new HashTable(reader);
            streamsCache = SimpleCache.CreateStruct(() =>
            {
                Dictionary<string, int> streams = new Dictionary<string, int>();
                IBinaryReader stringsReader = StringsStream.Duplicate();

                foreach (var kvp in HashTable.Dictionary)
                {
                    stringsReader.Position = kvp.Key;
                    streams.Add(stringsReader.ReadCString(), (int)kvp.Value);
                }
                return streams;
            });
            streamsUppercaseCache = SimpleCache.CreateStruct(() =>
            {
                Dictionary<string, int> streams = new Dictionary<string, int>();

                foreach (var kvp in Streams)
                    streams.Add(kvp.Key.ToUpperInvariant(), kvp.Value);
                return streams;
            });
        }

        /// <summary>
        /// Gets the stream that holds strings.
        /// </summary>
        public IBinaryReader StringsStream { get; private set; }

        /// <summary>
        /// Gets the hash table containing string offset and stream id.
        /// </summary>
        public HashTable HashTable { get; private set; }

        /// <summary>
        /// Gets the dictionary of streams given by name and its id.
        /// </summary>
        public Dictionary<string, int> Streams => streamsCache.Value;

        /// <summary>
        /// Gets the dictionary of streams given by uppercase name and its id.
        /// </summary>
        public Dictionary<string, int> StreamsUppercase => streamsUppercaseCache.Value;
    }
}
