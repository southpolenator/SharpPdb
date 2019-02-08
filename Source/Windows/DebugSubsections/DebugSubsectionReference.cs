namespace SharpPdb.Windows.DebugSubsections
{
    /// <summary>
    /// Internal debug subsection reference structure.
    /// </summary>
    public struct DebugSubsectionReference
    {
        /// <summary>
        /// Offset of the debug subsection data in the stream.
        /// </summary>
        public long DataOffset;

        /// <summary>
        /// Debug subsection data length in bytes.
        /// </summary>
        public uint DataLen;

        /// <summary>
        /// Debug subsection kind.
        /// </summary>
        public DebugSubsectionKind Kind;
    }
}
