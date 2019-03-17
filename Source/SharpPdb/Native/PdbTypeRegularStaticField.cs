using SharpPdb.Native.Types;
using SharpPdb.Windows.SymbolRecords;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents regular static field of the type read from the PDB file.
    /// </summary>
    public class PdbTypeRegularStaticField : PdbTypeStaticField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeRegularStaticField"/> class.
        /// </summary>
        /// <param name="containerType">Type that contains this field.</param>
        /// <param name="staticDataMemberRecord">The static data member record.</param>
        /// <param name="data">Data symbol for this static field.</param>
        internal PdbTypeRegularStaticField(PdbUserDefinedType containerType, StaticDataMemberRecord staticDataMemberRecord, DataSymbol data)
            : base(containerType, staticDataMemberRecord)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the data symbol for this static field.
        /// </summary>
        public DataSymbol Data { get; private set; }


        /// <summary>
        /// Gets the offset portion of symbol address.
        /// </summary>
        public uint Offset => Data.Offset;

        /// <summary>
        /// Gets the segment portion of symbol address.
        /// </summary>
        public ushort Segment => Data.Segment;
    }
}
