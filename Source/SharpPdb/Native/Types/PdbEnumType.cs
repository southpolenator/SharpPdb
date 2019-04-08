using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents enumeration type read from PDB file.
    /// </summary>
    public class PdbEnumType : PdbUserDefinedType
    {
        #region SimpleCache delegates
        private Func<PdbEnumType, IReadOnlyList<PdbEnumeratorValue>> CallEnumerateValues = (t) => t.EnumerateValues();
        #endregion

        /// <summary>
        /// Cache for <see cref="Values"/> property.
        /// </summary>
        private SimpleCacheWithContext<IReadOnlyList<PdbEnumeratorValue>, PdbEnumType> valuesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="enumRecord">The enum record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbEnumType(PdbFileReader pdb, TypeIndex typeIndex, EnumRecord enumRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, enumRecord, modifierOptions, pdb[enumRecord.UnderlyingType].Size)
        {
            EnumRecord = enumRecord;
            valuesCache = SimpleCache.CreateWithContext(this, CallEnumerateValues);
        }

        /// <summary>
        /// Gets the enum record.
        /// </summary>
        public EnumRecord EnumRecord { get; private set; }

        /// <summary>
        /// Gets the underlying type of enum.
        /// </summary>
        public PdbType UnderlyingType => Pdb[EnumRecord.UnderlyingType];

        /// <summary>
        /// Gets the list of enumerator values.
        /// </summary>
        public IReadOnlyList<PdbEnumeratorValue> Values => valuesCache.Value;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"enum {Name} ({UnderlyingType})";
        }

        private IReadOnlyList<PdbEnumeratorValue> EnumerateValues()
        {
            List<PdbEnumeratorValue> values = new List<PdbEnumeratorValue>();

            foreach (var value in EnumerateFieldList().OfType<EnumeratorRecord>())
                values.Add(new PdbEnumeratorValue(this, value));
            return values;
        }
    }
}
