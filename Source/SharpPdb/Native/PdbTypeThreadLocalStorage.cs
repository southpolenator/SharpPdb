using SharpPdb.Native.Types;
using SharpPdb.Windows.SymbolRecords;
using SharpPdb.Windows.TypeRecords;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents thread local storage static field of the type read from the PDB file.
    /// </summary>
    public class PdbTypeThreadLocalStorage : PdbTypeStaticField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdbTypeThreadLocalStorage"/> class.
        /// </summary>
        /// <param name="containerType">Type that contains this field.</param>
        /// <param name="staticDataMemberRecord">The static data member record.</param>
        /// <param name="threadLocalData">The thread local data.</param>
        internal PdbTypeThreadLocalStorage(PdbUserDefinedType containerType, StaticDataMemberRecord staticDataMemberRecord, ThreadLocalDataSymbol threadLocalData)
            : base(containerType, staticDataMemberRecord)
        {
            ThreadLocalData = threadLocalData;
        }

        /// <summary>
        /// Gets the thread local data.
        /// </summary>
        public ThreadLocalDataSymbol ThreadLocalData { get; private set; }


        /// <summary>
        /// Gets the offset into thread local storage.
        /// </summary>
        public uint Offset => ThreadLocalData.Offset;

        /// <summary>
        /// Gets the segment of thread local storage.
        /// </summary>
        public ushort Segment => ThreadLocalData.Segment;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"({Access}) thread_local {Type.Name} {Name}";
        }
    }
}
