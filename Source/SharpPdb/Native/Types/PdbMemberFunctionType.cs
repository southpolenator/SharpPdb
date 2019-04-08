using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;
using SharpUtilities;
using System;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents member function type read from PDB file.
    /// </summary>
    public class PdbMemberFunctionType : PdbType
    {
        #region SimpleCache delegates
        private Func<PdbMemberFunctionType, PdbType[]> CallEnumerateArguments = (t) => t.EnumerateArguments();
        #endregion

        /// <summary>
        /// Cache for <see cref="Arguments"/> property.
        /// </summary>
        private SimpleCacheWithContext<PdbType[], PdbMemberFunctionType> argumentsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbMemberFunctionType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="memberFuctionRecord">The member function record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        internal PdbMemberFunctionType(PdbFileReader pdb, TypeIndex typeIndex, MemberFunctionRecord memberFuctionRecord, ModifierOptions modifierOptions)
            : base(pdb, typeIndex, modifierOptions, string.Empty, 0)
        {
            MemberFunctionRecord = memberFuctionRecord;
            argumentsCache = SimpleCache.CreateWithContext(this, CallEnumerateArguments);
        }

        /// <summary>
        /// Gets the member function record.
        /// </summary>
        public MemberFunctionRecord MemberFunctionRecord { get; private set; }

        /// <summary>
        /// Gets the calling convention of the function.
        /// </summary>
        public CallingConvention CallingConvention => MemberFunctionRecord.CallingConvention;

        /// <summary>
        /// Gets the number of parameters. This count includes the <c>this</c> parameter.
        /// </summary>
        public ushort ParameterCount => (ushort)(MemberFunctionRecord.ParameterCount + (ShouldAddThisAsParameter ? 1 : 0));

        /// <summary>
        /// Gets the PDB type of the value returned by the function.
        /// </summary>
        public PdbType ReturnType => Pdb[MemberFunctionRecord.ReturnType];

        /// <summary>
        /// Gets the function arguments.
        /// </summary>
        public PdbType[] Arguments => argumentsCache.Value;

        /// <summary>
        /// Gets the PDB type of the <c>this</c> parameter of the member function. A type of
        /// <c>void</c> indicates that the member function is static and has no <c>this</c> parameter.
        /// </summary>
        public PdbType ThisType => Pdb[MemberFunctionRecord.ThisType];

        /// <summary>
        /// Gets the Logical <c>this</c> adjuster for the method. Whenever a class element is
        /// referenced via the <c>this</c> pointer, <see cref="ThisPointerAdjustment"/> will be added to the resultant
        /// offset before referencing the element.
        /// </summary>
        public int ThisPointerAdjustment => MemberFunctionRecord.ThisPointerAdjustment;

        /// <summary>
        /// <c>true</c> if <c>this</c> should be added as a parameter - checks if function is static.
        /// </summary>
        private bool ShouldAddThisAsParameter => ThisType is PdbComplexPointerType;

        private PdbType[] EnumerateArguments()
        {
            TypeRecord typeRecord = Pdb.PdbFile.TpiStream[MemberFunctionRecord.ArgumentList];

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
