using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents base class of the user defined type read from the PDB file.
    /// </summary>
    public struct PdbTypeBaseClass
    {
        /// <summary>
        /// Offset of subobject that represents the base class within the structure.
        /// </summary>
        public ulong Offset;

        /// <summary>
        /// Type that represents the base class within the structure.
        /// </summary>
        public PdbType BaseType;

        /// <summary>
        /// Base class access.
        /// </summary>
        public MemberAccess Access;
    }
}
