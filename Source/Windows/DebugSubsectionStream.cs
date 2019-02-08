using SharpPdb.Windows.DebugSubsections;
using SharpPdb.Windows.Utility;
using SharpUtilities;
using System;
using System.Collections.Generic;

namespace SharpPdb.Windows
{
    /// <summary>
    /// Represents debug subsection stream.
    /// </summary>
    public class DebugSubsectionStream
    {
        /// <summary>
        /// List of all debug subsection references in this stream.
        /// </summary>
        private List<DebugSubsectionReference> references;

        /// <summary>
        /// Dictionary cache of debug subsections by its kind.
        /// </summary>
        private DictionaryCache<DebugSubsectionKind, DebugSubsection[]> debugSubsectionsByKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolStream"/> class.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <param name="end">End of the symbol stream in binary reader. If it is less than 0 or bigger than binary reader length, it will be read fully.</param>
        public DebugSubsectionStream(IBinaryReader reader, long end = -1)
        {
            Reader = reader;
            references = new List<DebugSubsectionReference>();

            long position = reader.Position;
            if (end < 0 || end > reader.Length)
                end = reader.Length;

            while (position < end)
            {
                DebugSubsectionKind kind = (DebugSubsectionKind)reader.ReadUint();
                uint dataLen = reader.ReadUint();

                references.Add(new DebugSubsectionReference
                {
                    DataOffset = position + 8,
                    Kind = kind,
                    DataLen = dataLen,
                });
                position += dataLen + 8;
                reader.ReadFake(dataLen);
            }

            debugSubsectionsByKind = new DictionaryCache<DebugSubsectionKind, DebugSubsection[]>(GetDebugSubsectionsByKind);
        }

        /// <summary>
        /// Gets the stream binary reader.
        /// </summary>
        public IBinaryReader Reader { get; private set; }

        /// <summary>
        /// Gets the read-only list of all debug subsections references in this stream.
        /// </summary>
        public IReadOnlyList<DebugSubsectionReference> References => references;

        /// <summary>
        /// Indexing operator for getting all debug subsections of the given kind.
        /// </summary>
        /// <param name="kind">Debug subsection kind that should be parsed from this stream.</param>
        /// <returns>Array of debug subsections for the specified debug subsection kind.</returns>
        public DebugSubsection[] this[DebugSubsectionKind kind] => debugSubsectionsByKind[kind];

        /// <summary>
        /// Parses all debug subsections of the specified debug subsection kind.
        /// </summary>
        /// <param name="kind">Debug subsection kind.</param>
        /// <returns>Array of debug subsections for the specified debug subsection kind.</returns>
        private DebugSubsection[] GetDebugSubsectionsByKind(DebugSubsectionKind kind)
        {
            List<DebugSubsection> symbols = new List<DebugSubsection>();

            for (int i = 0; i < references.Count; i++)
                if (references[i].Kind == kind)
                    symbols.Add(GetDebugSubsection(i));
            return symbols.ToArray();
        }

        /// <summary>
        /// Reads debug subsections from references for the specified index.
        /// </summary>
        /// <param name="index">Index of the debug subsection.</param>
        private DebugSubsection GetDebugSubsection(int index)
        {
            // Since DictionaryCache is allowing only single thread to call this function, we don't need to lock reader here.
            DebugSubsectionReference reference = references[index];

            Reader.Position = reference.DataOffset;
            switch (reference.Kind)
            {
                case DebugSubsectionKind.Lines:
                    return LinesSubsection.Read(Reader, reference.Kind, reference.DataLen);
                case DebugSubsectionKind.FileChecksums:
                    return FileChecksumSubsection.Read(Reader, reference.Kind);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
