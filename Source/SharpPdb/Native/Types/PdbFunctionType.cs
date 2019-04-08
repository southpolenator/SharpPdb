using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;
using SharpUtilities;
using System;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents function type read from PDB file.
    /// </summary>
    public class PdbFunctionType : PdbType
    {
        #region SimpleCache delegates
        private Func<PdbFunctionType, PdbType[]> CallEnumerateArguments = (t) => t.EnumerateArguments();
        #endregion

        /// <summary>
        /// Cache for <see cref="Arguments"/> property.
        /// </summary>
        private SimpleCacheWithContext<PdbType[], PdbFunctionType> argumentsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFunctionType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="procedureRecord">The procedure record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbFunctionType(PdbFileReader pdb, TypeIndex typeIndex, ProcedureRecord procedureRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, modifierOptions, string.Empty, 0)
        {
            ProcedureRecord = procedureRecord;
            argumentsCache = SimpleCache.CreateWithContext(this, CallEnumerateArguments);
        }

        /// <summary>
        /// Gets the procedure record.
        /// </summary>
        public ProcedureRecord ProcedureRecord { get; private set; }

        /// <summary>
        /// Gets the calling convention of the function.
        /// </summary>
        public CallingConvention CallingConvention => ProcedureRecord.CallingConvention;

        /// <summary>
        /// Gets the number of parameters.
        /// </summary>
        public ushort ParameterCount => ProcedureRecord.ParameterCount;

        /// <summary>
        /// Gets the PDB type of the value returned by the function.
        /// </summary>
        public PdbType ReturnType => Pdb[ProcedureRecord.ReturnType];

        /// <summary>
        /// Gets the function arguments.
        /// </summary>
        public PdbType[] Arguments => argumentsCache.Value;

        private PdbType[] EnumerateArguments()
        {
            TypeRecord typeRecord = Pdb.PdbFile.TpiStream[ProcedureRecord.ArgumentList];

            if (typeRecord is ArgumentListRecord argumentList)
            {
                PdbType[] arguments = new PdbType[argumentList.Arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                    arguments[i] = Pdb[argumentList.Arguments[i]];
                return arguments;
            }
            return new PdbType[0];
        }
    }
}
