namespace SharpPdb.Managed
{
    /// <summary>
    /// Represents sequence point referenced in PDB file.
    /// </summary>
    public interface IPdbSequencePoint
    {
        /// <summary>
        /// Gets the source file that contains this sequence point.
        /// </summary>
        IPdbSource Source { get; }

        /// <summary>
        /// Gets the start line number.
        /// </summary>
        int StartLine { get; }

        /// <summary>
        /// Gets the end line number.
        /// </summary>
        int EndLine { get; }

        /// <summary>
        /// Gets the start column number.
        /// </summary>
        int StartColumn { get; }

        /// <summary>
        /// Gets the end column number.
        /// </summary>
        int EndColumn { get; }

        /// <summary>
        /// Gets the IL offset of this sequence point.
        /// </summary>
        int Offset { get; }
    }
}
