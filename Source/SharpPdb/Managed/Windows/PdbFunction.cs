using SharpPdb.Windows.DBI;
using SharpPdb.Windows.DebugSubsections;
using SharpPdb.Windows.SymbolRecords;
using SharpUtilities;
using System.Collections.Generic;
using System.Linq;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents function referenced in Windows PDB file.
    /// </summary>
    public class PdbFunction : IPdbFunction
    {
        /// <summary>
        /// Cache for <see cref="LocalScopes"/> property
        /// </summary>
        private SimpleCacheStruct<IPdbLocalScope[]> localScopesCache;

        /// <summary>
        /// Cache for <see cref="SequencePoints"/> property.
        /// </summary>
        private SimpleCacheStruct<List<IPdbSequencePoint>> sequencePointsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFunction"/> class.
        /// </summary>
        /// <param name="pdbFile">Portable PDB file reader.</param>
        /// <param name="procedure">Managed procedure symbol from PDB.</param>
        /// <param name="dbiModule">DBI module descriptor from PDB.</param>
        internal PdbFunction(PdbFile pdbFile, ManagedProcedureSymbol procedure, DbiModuleDescriptor dbiModule)
        {
            PdbFile = pdbFile;
            Procedure = procedure;
            DbiModule = dbiModule;
            localScopesCache = SimpleCache.CreateStruct(() =>
            {
                IEnumerable<BlockSymbol> blocks = Procedure.Children.OfType<BlockSymbol>();
                int count = blocks.Count();
                IPdbLocalScope[] scopes = new IPdbLocalScope[count];
                int i = 0;

                foreach (BlockSymbol block in blocks)
                    scopes[i++] = new PdbLocalScope(this, block);
                return scopes;
            });
            sequencePointsCache = SimpleCache.CreateStruct(() =>
            {
                var checksums = DbiModule.DebugSubsectionStream[DebugSubsectionKind.FileChecksums];
                var linesSubsections = dbiModule.DebugSubsectionStream[DebugSubsectionKind.Lines];
                List<IPdbSequencePoint> sequencePoints = new List<IPdbSequencePoint>();

                foreach (LinesSubsection linesSubsection in linesSubsections)
                {
                    foreach (var file in linesSubsection.Files)
                    {
                        var checksum = (FileChecksumSubsection)checksums[file.Index];
                        var source = PdbFile[checksum];

                        foreach (var line in file.Lines)
                        {
                            sequencePoints.Add(new PdbSequencePoint(this, source, line));
                        }
                    }
                }

                return sequencePoints;
            });
        }

        /// <summary>
        /// Gets the Windows PDB file reader.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets the managed procedure symbol.
        /// </summary>
        public ManagedProcedureSymbol Procedure { get; private set; }

        /// <summary>
        /// Gets the DBI module descriptor.
        /// </summary>
        public DbiModuleDescriptor DbiModule { get; private set; }

        /// <summary>
        /// Gets the method token.
        /// </summary>
        public int Token => (int)Procedure.FunctionType.Index;

        /// <summary>
        /// Gets the list of local scopes in this function.
        /// </summary>
        public IReadOnlyList<IPdbLocalScope> LocalScopes => localScopesCache.Value;

        /// <summary>
        /// Gets the list of sequence points in this function.
        /// </summary>
        public IReadOnlyList<IPdbSequencePoint> SequencePoints => sequencePointsCache.Value;
    }
}
