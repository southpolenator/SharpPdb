using SharpPdb.Native.Types;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents PDB enum type single enumerator value.
    /// </summary>
    public class PdbEnumeratorValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbType"/> class.
        /// </summary>
        /// <param name="enumType">Enum type that contains this enumerator value.</param>
        /// <param name="enumeratorRecord">The enumerator record.</param>
        internal PdbEnumeratorValue(PdbEnumType enumType, EnumeratorRecord enumeratorRecord)
        {
            EnumType = enumType;
            EnumeratorRecord = enumeratorRecord;
        }

        /// <summary>
        /// Gets the enum type that contains this enumerator value.
        /// </summary>
        public PdbEnumType EnumType { get; private set; }

        /// <summary>
        /// Gets the enumerator record.
        /// </summary>
        public EnumeratorRecord EnumeratorRecord { get; private set; }

        /// <summary>
        /// Gets the enumerator name.
        /// </summary>
        public string Name => EnumeratorRecord.Name;

        /// <summary>
        /// Gets the enumerator value.
        /// </summary>
        public object Value => EnumeratorRecord.Value;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Name} = {Value}";
        }
    }
}
