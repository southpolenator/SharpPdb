namespace SharpPdb.Windows.SymbolRecords
{
    /// <summary>
    /// Internal symbol reference structure.
    /// </summary>
    public struct SymbolRecordReference
    {
        /// <summary>
        /// Offset of the symbol record data in the stream.
        /// </summary>
        public uint DataOffset;

        /// <summary>
        /// Symbol record data length in bytes.
        /// </summary>
        public ushort DataLen;

        /// <summary>
        /// Symbol record kind.
        /// </summary>
        public SymbolRecordKind Kind;
    }
}
