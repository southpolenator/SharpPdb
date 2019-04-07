using SharpPdb.Native.Types;
using SharpPdb.Windows;
using SharpPdb.Windows.SymbolRecords;
using SharpPdb.Windows.TypeRecords;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpPdb.Native
{
    /// <summary>
    /// Represents Native Windows PDB file reader.
    /// </summary>
    public class PdbFileReader : IDisposable
    {
        /// <summary>
        /// Cache of types by type index.
        /// </summary>
        private DictionaryCache<TypeIndex, PdbType> typesByIndex;

        /// <summary>
        /// Cache for <see cref="UserDefinedTypes"/> property.
        /// </summary>
        private SimpleCacheStruct<IReadOnlyList<PdbType>> userDefinedTypesCache;

        /// <summary>
        /// Cache for <see cref="GlobalVariables"/> property.
        /// </summary>
        private SimpleCacheStruct<PdbGlobalVariable[]> globalVarablesCache;

        /// <summary>
        /// Cache for <see cref="PublicSymbols"/> property.
        /// </summary>
        private SimpleCacheStruct<PdbPublicSymbol[]> publicSymbolsCache;

        /// <summary>
        /// Cache for <see cref="Functions"/> property.
        /// </summary>
        private SimpleCacheStruct<List<PdbFunction>> functionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFileReader"/> class.
        /// </summary>
        /// <param name="pdbPath">Path to PDB file.</param>
        public PdbFileReader(string pdbPath)
            : this(new PdbFile(pdbPath))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFileReader"/> class.
        /// </summary>
        /// <param name="file">File loaded into memory. Note that file will be closed when instance of this type is disposed.</param>
        public PdbFileReader(MemoryLoadedFile file)
            : this(new PdbFile(file))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFileReader"/> class.
        /// </summary>
        /// <param name="reader">Binary stream reader of the PDB file.</param>
        public PdbFileReader(IBinaryReader reader)
            : this(new PdbFile(reader))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdbFileReader"/> class.
        /// </summary>
        /// <param name="pdbFile">Opened PDB file.</param>
        private PdbFileReader(PdbFile pdbFile)
        {
            PdbFile = pdbFile;
            typesByIndex = new DictionaryCache<TypeIndex, PdbType>(CreateType);
            userDefinedTypesCache = SimpleCache.CreateStruct(() =>
            {
                List<PdbType> types = new List<PdbType>();
                var references = PdbFile.TpiStream.References;
                TypeLeafKind[] allowedKinds = ClassRecord.Kinds.Concat(UnionRecord.Kinds).Concat(EnumRecord.Kinds).ToArray();

                for (int i = 0; i < references.Count; i++)
                    if (allowedKinds.Contains(references[i].Kind))
                    {
                        TypeIndex typeIndex = TypeIndex.FromArrayIndex(i);
                        TypeRecord typeRecord = PdbFile.TpiStream[typeIndex];
                        PdbType pdbType = typesByIndex[typeIndex];

                        if (typeRecord is TagRecord tagRecord)
                        {
                            // Check if it is forward reference and if it has been resolved.
                            PdbUserDefinedType pdbUserDefinedType = (PdbUserDefinedType)pdbType;

                            if (pdbUserDefinedType.TagRecord != tagRecord)
                                continue;
                        }
                        types.Add(pdbType);
                    }
                return (IReadOnlyList<PdbType>)types;
            });
            globalVarablesCache = SimpleCache.CreateStruct(() =>
            {
                var data = PdbFile.GlobalsStream.Data;
                PdbGlobalVariable[] globalVariables = new PdbGlobalVariable[data.Count];

                for (int i = 0; i < data.Count; i++)
                    globalVariables[i] = new PdbGlobalVariable(this, data[i]);
                return globalVariables;
            });
            publicSymbolsCache = SimpleCache.CreateStruct(() =>
            {
                PdbPublicSymbol[] publicSymbols = new PdbPublicSymbol[PdbFile.PublicsStream.PublicSymbols.Count];

                for (int i = 0; i < publicSymbols.Length; i++)
                    publicSymbols[i] = new PdbPublicSymbol(this, PdbFile.PublicsStream.PublicSymbols[i]);
                return publicSymbols;
            });
            functionsCache = SimpleCache.CreateStruct(() =>
            {
                List<PdbFunction> functions = new List<PdbFunction>();
                var references = PdbFile.PdbSymbolStream?.References;
                var modules = PdbFile.DbiStream?.Modules;

                if (references != null && modules != null)
                {
                    HashSet<uint>[] selectedFunctions = new HashSet<uint>[modules.Count];

                    for (int i = 0; i < references.Count; i++)
                    {
                        ProcedureSymbol procedure = null;

                        switch (references[i].Kind)
                        {
                            // ProcedureSymbol
                            case SymbolRecordKind.S_GPROC32:
                            case SymbolRecordKind.S_LPROC32:
                            case SymbolRecordKind.S_GPROC32_ID:
                            case SymbolRecordKind.S_LPROC32_ID:
                            case SymbolRecordKind.S_LPROC32_DPC:
                            case SymbolRecordKind.S_LPROC32_DPC_ID:
                                procedure = PdbFile.PdbSymbolStream[i] as ProcedureSymbol;
                                break;
                            // ProcedureReferenceSymbol
                            case SymbolRecordKind.S_PROCREF:
                            case SymbolRecordKind.S_LPROCREF:
                                {
                                    ProcedureReferenceSymbol procedureReference = PdbFile.PdbSymbolStream[i] as ProcedureReferenceSymbol;
                                    int moduleIndex = procedureReference.Module - 1;

                                    if (moduleIndex >= 0 && moduleIndex < modules.Count)
                                    {
                                        var module = modules[moduleIndex];

                                        if (selectedFunctions[moduleIndex] == null)
                                            selectedFunctions[moduleIndex] = new HashSet<uint>();
                                        if (!selectedFunctions[moduleIndex].Contains(procedureReference.Offset)
                                            && module.LocalSymbolStream.TryGetSymbolRecordByOffset(procedureReference.Offset, out SymbolRecord procedureSymbol))
                                        {
                                            procedure = procedureSymbol as ProcedureSymbol;
                                            selectedFunctions[moduleIndex].Add(procedureReference.Offset);
                                        }
                                    }
                                }
                                break;
                        }

                        if (procedure != null)
                            functions.Add(new PdbFunction(this, procedure));
                    }
                }
                return functions;
            });
        }

        /// <summary>
        /// Gets the Native Windows PDB reader.
        /// </summary>
        public PdbFile PdbFile { get; private set; }

        /// <summary>
        /// Gets the user defined types from PDB file.
        /// </summary>
        public IReadOnlyList<PdbType> UserDefinedTypes => userDefinedTypesCache.Value;

        /// <summary>
        /// Gets the global variables from PDB file.
        /// </summary>
        public PdbGlobalVariable[] GlobalVariables => globalVarablesCache.Value;

        /// <summary>
        /// Gets the public symbols from PDB file.
        /// </summary>
        public PdbPublicSymbol[] PublicSymbols => publicSymbolsCache.Value;

        /// <summary>
        /// Gets the functions from PDB file.
        /// </summary>
        public IReadOnlyList<PdbFunction> Functions => functionsCache.Value;

        /// <summary>
        /// Gets the <see cref="PdbType"/> at the specified index.
        /// </summary>
        /// <param name="typeIndex">The type index.</param>
        internal PdbType this[TypeIndex typeIndex] => typesByIndex[typeIndex];

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            PdbFile?.Dispose();
        }

        /// <summary>
        /// Creates <see cref="PdbType"/> read from <see cref="PdbFile"/> for the specified type index.
        /// </summary>
        /// <param name="typeIndex">Type index.</param>
        private PdbType CreateType(TypeIndex typeIndex)
        {
            return CreateType(typeIndex, ModifierOptions.None, typeIndex);
        }

        /// <summary>
        /// Creates <see cref="PdbType"/> read from <see cref="PdbFile"/> for the specified type index.
        /// </summary>
        /// <param name="typeIndex">Type index.</param>
        /// <param name="modifierOptions">Modifier options for new type.</param>
        /// <param name="originalTypeIndex">Original type index of the PDB type.</param>
        private PdbType CreateType(TypeIndex typeIndex, ModifierOptions modifierOptions, TypeIndex originalTypeIndex)
        {
            if (typeIndex.IsSimple)
            {
                if (typeIndex.SimpleMode != SimpleTypeMode.Direct)
                    return new PdbSimplePointerType(this, typeIndex, modifierOptions);
                return new PdbSimpleType(this, typeIndex, modifierOptions);
            }

            TypeRecord typeRecord = PdbFile.TpiStream[typeIndex];

            if (typeRecord is ModifierRecord modifierRecord)
                return CreateType(modifierRecord.ModifiedType, modifierRecord.Modifiers, originalTypeIndex);

            if (typeRecord is TagRecord tagRecord && tagRecord.IsForwardReference)
            {
                // Resolve forward references
                TagRecord resolvedRecord = null;

                if (PdbFile.TpiStream.HashTable != null)
                {
                    TagRecord FindUdtByHash(uint hash)
                    {
                        var bucket = PdbFile.TpiStream.HashTable[hash % PdbFile.TpiStream.Header.HashBucketsCount];

                        while (bucket != null)
                        {
                            TypeRecord record = PdbFile.TpiStream[bucket.TypeIndex];

                            if (record is TagRecord tag && tagRecord.Name == tag.Name)
                                return tag;
                            bucket = bucket.Next;
                        }
                        return null;
                    }

                    resolvedRecord = FindUdtByHash(HashTable.HashStringV1(tagRecord.Name));
                    if (resolvedRecord == null && tagRecord.HasUniqueName)
                        resolvedRecord = FindUdtByHash(HashTable.HashStringV1(tagRecord.UniqueName));
                }
                else
                    resolvedRecord = PdbFile.TpiStream[typeRecord.Kind].OfType<TagRecord>().LastOrDefault(r => !r.IsForwardReference && r.UniqueName == tagRecord.UniqueName);
                if (resolvedRecord != null)
                    typeRecord = resolvedRecord;
            }

            if (typeRecord is ClassRecord classRecord)
                return new PdbClassType(this, originalTypeIndex, classRecord, modifierOptions);
            if (typeRecord is UnionRecord unionRecord)
                return new PdbUnionType(this, originalTypeIndex, unionRecord, modifierOptions);
            if (typeRecord is EnumRecord enumRecord)
                return new PdbEnumType(this, originalTypeIndex, enumRecord, modifierOptions);
            if (typeRecord is ArrayRecord arrayRecord)
                return new PdbArrayType(this, originalTypeIndex, arrayRecord, modifierOptions);
            if (typeRecord is PointerRecord pointerRecord)
                return new PdbComplexPointerType(this, originalTypeIndex, pointerRecord, modifierOptions);
            if (typeRecord is ProcedureRecord procedureRecord)
                return new PdbFunctionType(this, originalTypeIndex, procedureRecord, modifierOptions);
            if (typeRecord is MemberFunctionRecord memberFunctionRecord)
                return new PdbMemberFunctionType(this, originalTypeIndex, memberFunctionRecord, modifierOptions);

#if DEBUG
            throw new NotImplementedException();
#else
            return null;
#endif
        }
    }
}
