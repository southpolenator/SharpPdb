using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents virtual base class of the user defined type read from the PDB file.
    /// </summary>
    public struct PdbTypeVirtualBaseClass
    {
        /// <summary>
        /// Type that represents the base class within the structure.
        /// </summary>
        public PdbType BaseType;

        /// <summary>
        /// Type of the virtual base pointer for this base.
        /// </summary>
        public PdbType VirtualBasePointerType;

        /// <summary>
        /// Base class access.
        /// </summary>
        public MemberAccess Access;

        /// <summary>
        /// Is this virtual base class direct or indirect.
        /// </summary>
        public bool IsDirect;

        /// <summary>
        /// Gets the offset of the virtual base pointer from the
        /// address point of the class for this virtual base.
        /// </summary>
        public ulong VirtualBasePointerOffset;

        /// <summary>
        /// Gets the index into the virtual base displacement
        /// table of the entry that contains the displacement of the virtual base.
        /// The displacement is relative to the address point of the class plus <see cref="VirtualBasePointerOffset"/>.
        /// </summary>
        public ulong VirtualTableIndex;
    }
}
