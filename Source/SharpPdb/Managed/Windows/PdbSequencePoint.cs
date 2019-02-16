using SharpPdb.Windows.DebugSubsections;

namespace SharpPdb.Managed.Windows
{
    /// <summary>
    /// Represents sequence point referenced in Windows PDB file.
    /// </summary>
    public class PdbSequencePoint : IPdbSequencePoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbSequencePoint"/> class.
        /// </summary>
        /// <param name="function">Function that contains this sequence point.</param>
        /// <param name="source">Source file that contains this sequence point.</param>
        /// <param name="line">Parsed line object that describes this sequence point.</param>
        internal PdbSequencePoint(PdbFunction function, PdbSource source, LinesSubsection.Line line)
        {
            Function = function;
            Source = source;
            Line = line;
        }

        /// <summary>
        /// Gets the function that contains this sequence point.
        /// </summary>
        public PdbFunction Function { get; private set; }

        /// <summary>
        /// Gets the source file that contains this sequence point.
        /// </summary>
        public PdbSource Source { get; private set; }

        /// <summary>
        /// Gets the parsed line object that describes this sequence point.
        /// </summary>
        public LinesSubsection.Line Line { get; private set; }

        /// <summary>
        /// Gets the source file that contains this sequence point.
        /// </summary>
        IPdbSource IPdbSequencePoint.Source => Source;

        /// <summary>
        /// Gets the start line number.
        /// </summary>
        public int StartLine => (int)Line.LineStart;

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        public int EndLine => (int)Line.LineEnd;

        /// <summary>
        /// Gets the start column number.
        /// </summary>
        public int StartColumn => (int)Line.ColumnStart;

        /// <summary>
        /// Gets the end column number.
        /// </summary>
        public int EndColumn => (int)Line.ColumnEnd;

        /// <summary>
        /// Gets the IL offset of this sequence point.
        /// </summary>
        public int Offset => (int)Line.Offset;
    }
}
