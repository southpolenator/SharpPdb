using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpPdb.Windows.PIS
{
    /// <summary>
    /// Represents PDB info stream.
    /// </summary>
    public class InfoStream
    {
        /// <summary>
        /// Cache for <see cref="NamesMap"/> property.
        /// </summary>
        private SimpleCacheStruct<PdbStringTable> namesMapCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoStream"/> class.
        /// </summary>
        /// <param name="stream">PDB stream that contains PDB info stream.</param>
        public InfoStream(PdbStream stream)
        {
            Stream = stream;
            stream.Reader.Position = 0;
            if (Stream.Length < InfoStreamHeader.Size)
                throw new Exception("PDB info stream does not contain a header.");
            Header = InfoStreamHeader.Read(stream.Reader);
            if (Header.Version != InfoStreamVersion.VC70 && Header.Version != InfoStreamVersion.VC80 && Header.Version != InfoStreamVersion.VC110 && Header.Version != InfoStreamVersion.VC140)
                throw new Exception("Unsupported PDB info stream version.");
            NamedStreamMap = new NamedStreamMap(stream.Reader);
            namesMapCache = SimpleCache.CreateStruct(() =>
            {
                int namesMapStreamIndex;

                if (!NamedStreamMap.Streams.TryGetValue("/names", out namesMapStreamIndex))
                    return null;

                PdbStream namesMapStream = Stream.File.GetStream(namesMapStreamIndex);

                if (namesMapStream == null)
                    return null;
                return new PdbStringTable(namesMapStream.Reader);
            });

            // Read feature signatures
            bool stop = false;
            List<PdbFeatures> allFeatures = new List<PdbFeatures>();

            while (!stop && stream.Reader.BytesRemaining > 0)
            {
                PdbFeatureSignature signature = (PdbFeatureSignature)stream.Reader.ReadUint();
                PdbFeatures features = PdbFeatures.None;

                switch (signature)
                {
                    case PdbFeatureSignature.VC110:
                        stop = true;
                        features |= PdbFeatures.ContainsIdStream;
                        break;
                    case PdbFeatureSignature.VC140:
                        features |= PdbFeatures.ContainsIdStream;
                        break;
                    case PdbFeatureSignature.NoTypeMerge:
                        features |= PdbFeatures.NoTypeMerging;
                        break;
                    case PdbFeatureSignature.MinimalDebugInfo:
                        features |= PdbFeatures.MinimalDebugInfo;
                        break;
                }
                allFeatures.Add(features);
            }
            Features = allFeatures.ToArray();
        }

        /// <summary>
        /// Gets the associated PDB stream.
        /// </summary>
        public PdbStream Stream { get; private set; }

        /// <summary>
        /// Gets DBI stream header.
        /// </summary>
        public InfoStreamHeader Header { get; private set; }

        /// <summary>
        /// Gets the map of named streams.
        /// </summary>
        public NamedStreamMap NamedStreamMap { get; private set; }

        /// <summary>
        /// Gets the PDB features.
        /// </summary>
        public PdbFeatures[] Features { get; private set; }

        /// <summary>
        /// Gets the string table from the <c>/names</c> stream if exists or <c>null</c> otherwise.
        /// </summary>
        public PdbStringTable NamesMap => namesMapCache.Value;
    }
}
