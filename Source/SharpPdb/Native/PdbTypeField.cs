using SharpPdb.Native.Types;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents field of the type read from the PDB file.
    /// </summary>
    public class PdbTypeField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeField"/> class.
        /// </summary>
        /// <param name="containerType">Type that contains this field.</param>
        /// <param name="dataMemberRecord">The data member record.</param>
        internal PdbTypeField(PdbUserDefinedType containerType, DataMemberRecord dataMemberRecord)
        {
            ContainerType = containerType;
            DataMemberRecord = dataMemberRecord;
        }

        /// <summary>
        /// Gets the type that contains this field.
        /// </summary>
        public PdbUserDefinedType ContainerType { get; private set; }

        /// <summary>
        /// Gets the data member record.
        /// </summary>
        public DataMemberRecord DataMemberRecord { get; private set; }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string Name => DataMemberRecord.Name;

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public virtual PdbType Type => ContainerType.Pdb[DataMemberRecord.Type];

        /// <summary>
        /// Gets the offset of field in the <see cref="ContainerType"/>.
        /// </summary>
        public ulong Offset => DataMemberRecord.FieldOffset;

        /// <summary>
        /// Gets the field attributes.
        /// </summary>
        public MemberAttributes Attributes => DataMemberRecord.Attributes;

        /// <summary>
        /// Gets the field access.
        /// </summary>
        public MemberAccess Access => Attributes.Access;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({Access}) {Type.Name} {Name} ({Offset})";
        }
    }
}
