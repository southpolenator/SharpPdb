using SharpUtilities;
using System.Reflection.Metadata;

namespace SharpPdb.Managed.Portable
{
    /// <summary>
    /// Represents sequence point referenced in Portable PDB file.
    /// </summary>
    public class PdbSequencePoint : IPdbSequencePoint
    {
        /// <summary>
        /// Cache for <see cref="Source"/> property.
        /// </summary>
        private SimpleCacheStruct<IPdbSource> sourceCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSequencePoint"/> class.
        /// </summary>
        /// <param name="function">Function that contains this sequence point.</param>
        /// <param name="sequencePoint">Sequence point.</param>
        internal PdbSequencePoint(PdbFunction function, SequencePoint sequencePoint)
        {
            Function = function;
            SequencePoint = sequencePoint;
            sourceCache = SimpleCache.CreateStruct(() => Function.PdbFile[SequencePoint.Document]);
        }

        /// <summary>
        /// Gets the function that contains this sequence point.
        /// </summary>
        public PdbFunction Function { get; private set; }

        /// <summary>
        /// Gets the sequence point.
        /// </summary>
        public SequencePoint SequencePoint { get; private set; }

        /// <summary>
        /// Gets the source file that contains this sequence point.
        /// </summary>
        public IPdbSource Source => sourceCache.Value;

        /// <summary>
        /// Gets the start line number.
        /// </summary>
        public int StartLine => SequencePoint.StartLine;

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        public int EndLine => SequencePoint.EndLine;

        /// <summary>
        /// Gets the start column number.
        /// </summary>
        public int StartColumn => SequencePoint.StartColumn;

        /// <summary>
        /// Gets the end column number.
        /// </summary>
        public int EndColumn => SequencePoint.EndColumn;

        /// <summary>
        /// Gets the IL offset of this sequence point.
        /// </summary>
        public int Offset => SequencePoint.Offset;
    }
}
