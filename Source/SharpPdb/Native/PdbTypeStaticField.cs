using SharpPdb.Native.Types;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents static field of the type read from the PDB file.
    /// </summary>
    public class PdbTypeStaticField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeStaticField"/> class.
        /// </summary>
        /// <param name="containerType">Type that contains this field.</param>
        /// <param name="staticDataMemberRecord">The static data member record.</param>
        internal PdbTypeStaticField(PdbUserDefinedType containerType, StaticDataMemberRecord staticDataMemberRecord)
        {
            ContainerType = containerType;
            StaticDataMemberRecord = staticDataMemberRecord;
        }

        /// <summary>
        /// Gets the type that contains this field.
        /// </summary>
        public PdbUserDefinedType ContainerType { get; private set; }

        /// <summary>
        /// Gets the static data member record.
        /// </summary>
        public StaticDataMemberRecord StaticDataMemberRecord { get; private set; }

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string Name => StaticDataMemberRecord.Name.String;

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public PdbType Type => ContainerType.Pdb[StaticDataMemberRecord.Type];

        /// <summary>
        /// Gets the field attributes.
        /// </summary>
        public MemberAttributes Attributes => StaticDataMemberRecord.Attributes;

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
            return $"({Access}) {Type.Name} {Name}";
        }
    }
}
