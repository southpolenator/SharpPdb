using SharpPdb.Native.Types;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents bit field of the type read from the PDB file.
    /// </summary>
    public class PdbTypeBitField : PdbTypeField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeBitField"/> class.
        /// </summary>
        /// <param name="containerType">Type that contains this field.</param>
        /// <param name="dataMemberRecord">The data member record.</param>
        /// <param name="bitFieldRecord">The bit field record.</param>
        internal PdbTypeBitField(PdbUserDefinedType containerType, DataMemberRecord dataMemberRecord, BitFieldRecord bitFieldRecord)
            : base(containerType, dataMemberRecord)
        {
            BitFieldRecord = bitFieldRecord;
        }

        /// <summary>
        /// Gets the bit field record.
        /// </summary>
        public BitFieldRecord BitFieldRecord { get; private set; }

        /// <summary>
        /// Gets the length in bits of this field.
        /// </summary>
        public byte BitSize => BitFieldRecord.BitSize;

        /// <summary>
        /// Gets the starting position (from bit 0) of the object in the word.
        /// </summary>
        public byte BitOffset => BitFieldRecord.BitOffset;

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public override PdbType Type => ContainerType.Pdb[BitFieldRecord.Type];

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({Attributes.Access}) {Type.Name} {Name}:{BitSize} ({Offset} + {BitOffset})";
        }
    }
}
