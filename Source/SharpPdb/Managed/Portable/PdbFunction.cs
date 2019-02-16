using SharpUtilities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents function referenced in Portable PDB file.
    /// </summary>
    public class PdbFunction : IPdbFunction
    {
        /// <summary>
        /// Cache for <see cref="MethodDebugInformation"/> property.
        /// </summary>
        private SimpleCacheStruct<MethodDebugInformation> methodDebugInformationCache;

        /// <summary>
        /// Cache for <see cref="LocalScopes"/> property
        /// </summary>
        private SimpleCacheStruct<IPdbLocalScope[]> localScopesCache;

        /// <summary>
        /// Cache for <see cref="SequencePoints"/> property.
        /// </summary>
        private SimpleCacheStruct<IPdbSequencePoint[]> sequencePointsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFunction"/> class.
        /// </summary>
        /// <param name="pdbFile">Portable PDB file reader.</param>
        /// <param name="handle">Our metadata reader handle.</param>
        internal PdbFunction(PdbFile pdbFile, MethodDebugInformationHandle handle)
        {
            PdbFile = pdbFile;
            methodDebugInformationCache = SimpleCache.CreateStruct(() => pdbFile.Reader.GetMethodDebugInformation(handle));
            localScopesCache = SimpleCache.CreateStruct(() =>
            {
                var localScopes = pdbFile.Reader.GetLocalScopes(handle);
                IPdbLocalScope[] scopes = new IPdbLocalScope[localScopes.Count];
                int i = 0;

                foreach (var l in localScopes)
                    scopes[i++] = new PdbLocalScope(this, l);
                return scopes;
            });
            sequencePointsCache = SimpleCache.CreateStruct(() =>
            {
                var sequencePoints = MethodDebugInformation.GetSequencePoints();

                return sequencePoints.Select(sp => new PdbSequencePoint(this, sp)).OfType<IPdbSequencePoint>().ToArray();
            });
            Token = System.Reflection.Metadata.Ecma335.MetadataTokens.GetToken(handle.ToDefinitionHandle());
        }

        /// <summary>
        /// Gets the Portable PDB file reader.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets the method debug information.
        /// </summary>
        public MethodDebugInformation MethodDebugInformation => methodDebugInformationCache.Value;

        /// <summary>
        /// Gets the method token.
        /// </summary>
        public int Token { get; private set; }

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
