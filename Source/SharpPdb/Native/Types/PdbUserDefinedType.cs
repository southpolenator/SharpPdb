using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;
using SharpPdb.Windows.TypeRecords;
using SharpUtilities;
using System.Collections.Generic;
using System.Linq;

namespace SharpPdb.Native.Types
{
    /// <summary>
    /// Represents user defined type read from PDB file.
    /// </summary>
    public class PdbUserDefinedType : PdbType
    {
        /// <summary>
        /// Cache for <see cref="Fields"/> property.
        /// </summary>
        private SimpleCacheStruct<IReadOnlyList<PdbTypeField>> fieldsCache;

        /// <summary>
        /// Cache for <see cref="StaticFields"/> property.
        /// </summary>
        private SimpleCacheStruct<IReadOnlyList<PdbTypeStaticField>> staticFieldsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbType"/> class.
        /// </summary>
        /// <param name="pdb">The PDB file reader.</param>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="tagRecord">The tag record.</param>
        /// <param name="modifierOptions">The modifier options.</param>
        /// <param name="size">The type size in bytes.</param>
        internal PdbUserDefinedType(PdbFileReader pdb, TypeIndex typeIndex, TagRecord tagRecord, ModifierOptions modifierOptions, ulong size)
            : base(pdb, typeIndex, modifierOptions, tagRecord.Name, size)
        {
            TagRecord = tagRecord;
            fieldsCache = SimpleCache.CreateStruct(EnumerateFields);
            staticFieldsCache = SimpleCache.CreateStruct(EnumerateStaticFields);
        }

        /// <summary>
        /// Gets the tag record.
        /// </summary>
        public TagRecord TagRecord { get; private set; }

        /// <summary>
        /// Gets the unique type name.
        /// </summary>
        public string UniqueName => TagRecord.UniqueName;

        /// <summary>
        /// Gets the all fields from this type.
        /// </summary>
        public IReadOnlyList<PdbTypeField> Fields => fieldsCache.Value;

        /// <summary>
        /// Gets the all static fields from this type.
        /// </summary>
        public IReadOnlyList<PdbTypeStaticField> StaticFields => staticFieldsCache.Value;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.Nested"/>.
        /// </summary>
        public bool IsNested => TagRecord.IsNested;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.Scoped"/>.
        /// </summary>
        public bool IsScoped => (TagRecord.Options & ClassOptions.Scoped) == ClassOptions.Scoped;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.Packed"/>.
        /// </summary>
        public bool IsPacked => (TagRecord.Options & ClassOptions.Packed) == ClassOptions.Packed;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.HasOverloadedOperator"/>.
        /// </summary>
        public bool HasOverloadedOperator => (TagRecord.Options & ClassOptions.HasOverloadedOperator) == ClassOptions.HasOverloadedOperator;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.ContainsNestedClass"/>.
        /// </summary>
        public bool ContainsNestedClass => (TagRecord.Options & ClassOptions.ContainsNestedClass) == ClassOptions.ContainsNestedClass;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.HasOverloadedAssignmentOperator"/>.
        /// </summary>
        public bool HasOverloadedAssignmentOperator => (TagRecord.Options & ClassOptions.HasOverloadedAssignmentOperator) == ClassOptions.HasOverloadedAssignmentOperator;

        /// <summary>
        /// <c>true</c> if <see cref="TagRecord"/>.<see cref="TagRecord.Options"/> contain <see cref="ClassOptions.HasConstructorOrDestructor"/>.
        /// </summary>
        public bool HasConstructorOrDestructor => (TagRecord.Options & ClassOptions.HasConstructorOrDestructor) == ClassOptions.HasConstructorOrDestructor;

        /// <summary>
        /// Enumerates field list of <see cref="TagRecord"/>.
        /// </summary>
        protected IEnumerable<TypeRecord> EnumerateFieldList()
        {
            return EnumerateFieldList(Pdb.PdbFile, TagRecord.FieldList);
        }

        /// <summary>
        /// Creates a list of fields based on <see cref="TagRecord"/>.<see cref="TagRecord.FieldList"/>.
        /// </summary>
        private IReadOnlyList<PdbTypeField> EnumerateFields()
        {
            List<PdbTypeField> fields = new List<PdbTypeField>();

            foreach (TypeRecord field in EnumerateFieldList())
                if (field is DataMemberRecord dataMemberRecord)
                {
                    // Check if this field is stored as bits.
                    TypeIndex typeIndex = dataMemberRecord.Type;

                    if (!typeIndex.IsSimple)
                    {
                        TypeRecord fieldType = Pdb.PdbFile.TpiStream[typeIndex];

                        if (fieldType is BitFieldRecord bitFieldRecord)
                        {
                            fields.Add(new PdbTypeBitField(this, dataMemberRecord, bitFieldRecord));
                            continue;
                        }
                    }

                    // Add it as regular field.
                    fields.Add(new PdbTypeField(this, dataMemberRecord));
                }
            return fields;
        }

        /// <summary>
        /// Creates a list of static fields based on <see cref="TagRecord"/>.<see cref="TagRecord.FieldList"/>.
        /// </summary>
        private IReadOnlyList<PdbTypeStaticField> EnumerateStaticFields()
        {
            List<PdbTypeStaticField> fields = new List<PdbTypeStaticField>();

            foreach (TypeRecord field in EnumerateFieldList())
                if (field is StaticDataMemberRecord staticDataMemberRecord)
                {
                    // Check if static field is constant
                    string fullName = Name + "::" + staticDataMemberRecord.Name;
                    var globals = Pdb.PdbFile.GlobalsStream;
                    ConstantSymbol constant = null;
                    ThreadLocalDataSymbol threadLocalData = null;
                    DataSymbol data = null;

                    if (globals.HashBuckets != null)
                    {
                        uint hash = HashTable.HashStringV1(fullName);
                        var bucket = globals.HashBuckets[hash % (uint)globals.HashBuckets.Length];

                        for (int i = bucket.Start; i < bucket.End; i++)
                        {
                            SymbolRecord record = globals.Symbols[i];

                            if (record is ConstantSymbol c && c.Name == fullName)
                            {
                                constant = c;
                                break;
                            }
                            else if (record is ThreadLocalDataSymbol tls && tls.Name == fullName)
                            {
                                threadLocalData = tls;
                                break;
                            }
                            else if (record is DataSymbol ds && ds.Name == fullName)
                            {
                                data = ds;
                                break;
                            }

                            // TODO: bucket items are ordered by byte value. Is it comparable by string.CompareTo?
                        }
                    }
                    else
                    {
                        data = Pdb.PdbFile.GlobalsStream.Data.FirstOrDefault(d => d.Name == fullName);
                        if (data == null)
                        {
                            constant = Pdb.PdbFile.GlobalsStream.Constants.FirstOrDefault(c => c.Name == fullName);
                            if (constant == null)
                                threadLocalData = Pdb.PdbFile.GlobalsStream.ThreadLocalData.FirstOrDefault(t => t.Name == fullName);
                        }
                    }

                    // Create correct static field type
                    if (constant != null)
                        fields.Add(new PdbTypeConstant(this, staticDataMemberRecord, constant));
                    else if (threadLocalData != null)
                        fields.Add(new PdbTypeThreadLocalStorage(this, staticDataMemberRecord, threadLocalData));
                    else if (data != null)
                        fields.Add(new PdbTypeRegularStaticField(this, staticDataMemberRecord, data));
                    else
                        fields.Add(new PdbTypeStaticField(this, staticDataMemberRecord));
                }
            return fields;
        }

        private static IEnumerable<TypeRecord> EnumerateFieldList(PdbFile pdb, TypeIndex fieldListIndex)
        {
            while (fieldListIndex != TypeIndex.None)
            {
                TypeIndex nextFieldListIndex = TypeIndex.None;
                TypeRecord listRecord = pdb.TpiStream[fieldListIndex];
                if (listRecord is FieldListRecord fieldListRecord)
                    foreach (TypeRecord field in fieldListRecord.Fields)
                        if (field is ListContinuationRecord listContinuation)
                            nextFieldListIndex = listContinuation.ContinuationIndex;
                        else
                            yield return field;
                fieldListIndex = nextFieldListIndex;
            }
        }
    }
}
