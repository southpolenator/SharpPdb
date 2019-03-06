using DIA;
using SharpPdb.Common.Tests;
using SharpPdb.Windows.TypeRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SharpPdb.Windows.Tests.E2E
{
    public class DiaTests : TestBase
    {
        [Fact]
        public void Test1()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return;

            string pdbPath = GetPdbPath(1);
            IDiaDataSource dia = DiaLoader.CreateDiaSource();
            IDiaSession diaSession;

            dia.loadDataFromPdb(pdbPath);
            dia.openSession(out diaSession);
            using (PdbFile pdb = new PdbFile(pdbPath))
            {
                TestUdts(diaSession, pdb);
            }
        }

        private static void TestUdts(IDiaSession diaSession, PdbFile pdb)
        {
            // Extract all udts from PDB
            var typeRecordReferences = pdb.TpiStream.References;
            List<ClassRecord> classRecords = new List<ClassRecord>();
            List<UnionRecord> unionRecords = new List<UnionRecord>();

            for (int i = 0; i < typeRecordReferences.Count; i++)
                if (ClassRecord.Kinds.Contains(typeRecordReferences[i].Kind))
                    classRecords.Add((ClassRecord)pdb.TpiStream[TypeIndex.FromArrayIndex(i)]);
                else if (UnionRecord.Kinds.Contains(typeRecordReferences[i].Kind))
                    unionRecords.Add((UnionRecord)pdb.TpiStream[TypeIndex.FromArrayIndex(i)]);

            // Extract UDTs from DIA
            IDiaSymbol[] udts = diaSession.globalScope.GetChildren(SymTagEnum.UDT).ToArray();
            HashSet<string> checkedUdts = new HashSet<string>();
            List<Tuple<uint, TypeRecord>> checkedTypes = new List<Tuple<uint, TypeRecord>>();

            foreach (IDiaSymbol udt in udts)
            {
                string name = udt.name;

                if (udt.length > 0 && !checkedUdts.Contains(name))
                {
                    UdtKind udtKind = (UdtKind)udt.udtKind;
                    TagRecord record;

                    if (udtKind == UdtKind.Union)
                    {
                        UnionRecord unionRecord = unionRecords.FirstOrDefault(cr => !cr.IsForwardReference && cr.Name == name);

                        if (unionRecord == null)
                            unionRecord = unionRecords.FirstOrDefault(cr => cr.Name == name);
                        Assert.NotNull(unionRecord);
                        record = unionRecord;
                    }
                    else
                    {
                        ClassRecord classRecord = classRecords.FirstOrDefault(cr => !cr.IsForwardReference && cr.Name == name);

                        if (classRecord == null)
                            classRecord = classRecords.FirstOrDefault(cr => cr.Name == name);
                        Assert.NotNull(classRecord);
                        record = classRecord;
                    }

                    CompareTypes(udt, pdb, record, checkedTypes);
                    checkedUdts.Add(name);
                }
            }
        }

        private static void CompareTypes(IDiaSymbol diaSymbol, PdbFile pdb, TypeIndex pdbType, List<Tuple<uint, TypeRecord>> checkedTypes)
        {
            if (!pdbType.IsSimple)
            {
                CompareTypes(diaSymbol, pdb, pdb.TpiStream[pdbType], checkedTypes);
            }
            else
            {
                IDiaSymbol diaType = diaSymbol;

                if (pdbType.SimpleMode != SimpleTypeMode.Direct)
                {
                    Assert.True(diaSymbol.symTag == SymTagEnum.PointerType);
                    diaType = diaSymbol.type;
                }

                Assert.True(diaType.symTag == SymTagEnum.BaseType);

                switch (pdbType.SimpleKind)
                {
                    case SimpleTypeKind.None:
                        Assert.True(diaType.baseType == BasicType.NoType);
                        Assert.Equal(0U, diaType.length);
                        break;
                    case SimpleTypeKind.Void:
                        Assert.True(diaType.baseType == BasicType.Void);
                        Assert.Equal(0U, diaType.length);
                        break;
                    case SimpleTypeKind.HResult:
                        Assert.True(diaType.baseType == BasicType.Hresult);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.UnsignedCharacter:
                        Assert.True(diaType.baseType == BasicType.UInt);
                        Assert.Equal(1U, diaType.length);
                        break;
                    case SimpleTypeKind.NarrowCharacter:
                    case SimpleTypeKind.SignedCharacter:
                        Assert.True(diaType.baseType == BasicType.Char || diaType.baseType == BasicType.Int);
                        Assert.Equal(1U, diaType.length);
                        break;
                    case SimpleTypeKind.WideCharacter:
                        Assert.True(diaType.baseType == BasicType.WChar);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.Character16:
                        Assert.True(diaType.baseType == BasicType.Char16);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.Character32:
                        Assert.True(diaType.baseType == BasicType.Char32);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.SByte:
                        Assert.True(diaType.baseType == BasicType.Int);
                        Assert.Equal(1U, diaType.length);
                        break;
                    case SimpleTypeKind.Byte:
                        Assert.True(diaType.baseType == BasicType.UInt);
                        Assert.Equal(1U, diaType.length);
                        break;
                    case SimpleTypeKind.Int16Short:
                    case SimpleTypeKind.Int16:
                        Assert.True(diaType.baseType == BasicType.Int);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.UInt16:
                    case SimpleTypeKind.UInt16Short:
                        Assert.True(diaType.baseType == BasicType.UInt);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.Int32Long:
                    case SimpleTypeKind.Int32:
                        Assert.True(diaType.baseType == BasicType.Long || diaType.baseType == BasicType.Int);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.UInt32Long:
                    case SimpleTypeKind.UInt32:
                        Assert.True(diaType.baseType == BasicType.ULong || diaType.baseType == BasicType.UInt);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.Int64Quad:
                    case SimpleTypeKind.Int64:
                        Assert.True(diaType.baseType == BasicType.Long || diaType.baseType == BasicType.Int);
                        Assert.Equal(8U, diaType.length);
                        break;
                    case SimpleTypeKind.UInt64Quad:
                    case SimpleTypeKind.UInt64:
                        Assert.True(diaType.baseType == BasicType.ULong || diaType.baseType == BasicType.UInt);
                        Assert.Equal(8U, diaType.length);
                        break;
                    case SimpleTypeKind.Int128Oct:
                    case SimpleTypeKind.Int128:
                        Assert.True(diaType.baseType == BasicType.Long || diaType.baseType == BasicType.Int);
                        Assert.Equal(16U, diaType.length);
                        break;
                    case SimpleTypeKind.UInt128Oct:
                    case SimpleTypeKind.UInt128:
                        Assert.True(diaType.baseType == BasicType.ULong || diaType.baseType == BasicType.UInt);
                        Assert.Equal(16U, diaType.length);
                        break;
                    case SimpleTypeKind.Float16:
                        Assert.True(diaType.baseType == BasicType.Float);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.Float32:
                    case SimpleTypeKind.Float32PartialPrecision:
                        Assert.True(diaType.baseType == BasicType.Float);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.Float48:
                        Assert.True(diaType.baseType == BasicType.Float);
                        Assert.Equal(6U, diaType.length);
                        break;
                    case SimpleTypeKind.Float64:
                        Assert.True(diaType.baseType == BasicType.Float);
                        Assert.Equal(8U, diaType.length);
                        break;
                    case SimpleTypeKind.Float80:
                        Assert.True(diaType.baseType == BasicType.Float);
                        Assert.Equal(10U, diaType.length);
                        break;
                    case SimpleTypeKind.Float128:
                        Assert.True(diaType.baseType == BasicType.Float);
                        Assert.Equal(12U, diaType.length);
                        break;
                    case SimpleTypeKind.Complex16:
                        Assert.True(diaType.baseType == BasicType.Complex);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.Complex32:
                    case SimpleTypeKind.Complex32PartialPrecision:
                        Assert.True(diaType.baseType == BasicType.Complex);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.Complex48:
                        Assert.True(diaType.baseType == BasicType.Complex);
                        Assert.Equal(6U, diaType.length);
                        break;
                    case SimpleTypeKind.Complex64:
                        Assert.True(diaType.baseType == BasicType.Complex);
                        Assert.Equal(8U, diaType.length);
                        break;
                    case SimpleTypeKind.Complex80:
                        Assert.True(diaType.baseType == BasicType.Complex);
                        Assert.Equal(10U, diaType.length);
                        break;
                    case SimpleTypeKind.Complex128:
                        Assert.True(diaType.baseType == BasicType.Complex);
                        Assert.Equal(16U, diaType.length);
                        break;
                    case SimpleTypeKind.Boolean8:
                        Assert.True(diaType.baseType == BasicType.Bool);
                        Assert.Equal(1U, diaType.length);
                        break;
                    case SimpleTypeKind.Boolean16:
                        Assert.True(diaType.baseType == BasicType.Bool);
                        Assert.Equal(2U, diaType.length);
                        break;
                    case SimpleTypeKind.Boolean32:
                        Assert.True(diaType.baseType == BasicType.Bool);
                        Assert.Equal(4U, diaType.length);
                        break;
                    case SimpleTypeKind.Boolean64:
                        Assert.True(diaType.baseType == BasicType.Bool);
                        Assert.Equal(8U, diaType.length);
                        break;
                    case SimpleTypeKind.Boolean128:
                        Assert.True(diaType.baseType == BasicType.Bool);
                        Assert.Equal(16U, diaType.length);
                        break;
                    case SimpleTypeKind.NotTranslated:
                        Assert.True(diaType.baseType == BasicType.NoType);
                        Assert.Equal(0U, diaType.length);
                        break;
                    default:
                        throw new NotImplementedException($"Unexpected simple type: {pdbType.SimpleKind}, from type index: {pdbType}");
                }
            }
        }

        private static void CompareTypes(IDiaSymbol diaSymbol, PdbFile pdb, TypeRecord pdbType, List<Tuple<uint, TypeRecord>> checkedTypes)
        {
            uint diaSymbolId = diaSymbol.symIndexId;

            if (checkedTypes.Any(t => t.Item1 == diaSymbolId && t.Item2 == pdbType))
                return;
            checkedTypes.Add(Tuple.Create(diaSymbolId, pdbType));

            if (pdbType is TagRecord tagRecord)
            {
                Assert.Equal(diaSymbol.name, tagRecord.Name);
                if (tagRecord.IsForwardReference)
                {
                    // Find original type
                    Assert.True(tagRecord.HasUniqueName);
                    Assert.NotNull(tagRecord.UniqueName);
                    TagRecord resolvedRecord = pdb.TpiStream[pdbType.Kind].OfType<TagRecord>().LastOrDefault(r => !r.IsForwardReference && r.UniqueName == tagRecord.UniqueName);
                    if (resolvedRecord == null)
                    {
                        // Verify that DIA symbol is also empty
                        Assert.Equal(0U, diaSymbol.length);
                        return;
                    }
                    tagRecord = resolvedRecord;
                }

                if (tagRecord is UnionRecord unionRecord)
                {
                    Assert.Equal(diaSymbol.length, unionRecord.Size);
                }
                else if (tagRecord is ClassRecord classRecord)
                {
                    Assert.Equal(diaSymbol.length, classRecord.Size);
                    //if (diaSymbol.HasVTable() != (classRecord.VirtualTableShape != TypeIndex.None))
                    //{
                    //    var baseClasses = GetBaseClasses(pdb, classRecord).ToArray();
                    //    record = classRecord;
                    //}
                    //Assert.Equal(diaSymbol.HasVTable(), classRecord.VirtualTableShape != TypeIndex.None);
                }
                else if (tagRecord is EnumRecord enumRecord)
                {
                    CompareTypes(diaSymbol.type, pdb, enumRecord.UnderlyingType, checkedTypes);
                }
                else
                {
                    // TODO:
                    throw new NotImplementedException();
                }

                Assert.Equal(diaSymbol.nested, tagRecord.IsNested);
                Assert.Equal(diaSymbol.scoped, tagRecord.Options.HasFlag(ClassOptions.Scoped));
                Assert.Equal(diaSymbol.packed, tagRecord.Options.HasFlag(ClassOptions.Packed));
                Assert.Equal(diaSymbol.overloadedOperator, tagRecord.Options.HasFlag(ClassOptions.HasOverloadedOperator));
                Assert.Equal(diaSymbol.hasNestedTypes, tagRecord.Options.HasFlag(ClassOptions.ContainsNestedClass));
                Assert.Equal(diaSymbol.hasAssignmentOperator, tagRecord.Options.HasFlag(ClassOptions.HasOverloadedAssignmentOperator));
                Assert.Equal(diaSymbol.constructor, tagRecord.Options.HasFlag(ClassOptions.HasConstructorOrDestructor));

                if (!(tagRecord is EnumRecord))
                {
                    // Check fields
                    var diaFields = diaSymbol.GetChildren(SymTagEnum.Data).ToArray();
                    var fields = GetFields(pdb, tagRecord).ToArray();

                    Assert.Equal(diaFields.Length, fields.Length);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (fields[i] is DataMemberRecord dataMember)
                        {
                            Assert.Equal(diaFields[i].name, dataMember.Name);
                            Assert.Equal((ulong)diaFields[i].offset, dataMember.FieldOffset);
                            CompareTypes(diaFields[i].type, pdb, dataMember.Type, checkedTypes);
                        }
                        else if (fields[i] is StaticDataMemberRecord staticDataMember)
                        {
                            Assert.Equal(diaFields[i].name, staticDataMember.Name);
                            CompareTypes(diaFields[i].type, pdb, staticDataMember.Type, checkedTypes);
                        }
                    }
                }
            }
            else if (pdbType is ModifierRecord modifierRecord)
            {
                Assert.Equal(diaSymbol.constType, modifierRecord.Modifiers.HasFlag(ModifierOptions.Const));
                Assert.Equal(diaSymbol.unalignedType, modifierRecord.Modifiers.HasFlag(ModifierOptions.Unaligned));
                Assert.Equal(diaSymbol.volatileType, modifierRecord.Modifiers.HasFlag(ModifierOptions.Volatile));
                CompareTypes(diaSymbol, pdb, modifierRecord.ModifiedType, checkedTypes);
            }
            else if (pdbType is ArrayRecord arrayRecord)
            {
                Assert.True(diaSymbol.symTag == SymTagEnum.ArrayType);
                Assert.Equal(diaSymbol.length, arrayRecord.Size);
                CompareTypes(diaSymbol.type, pdb, arrayRecord.ElementType, checkedTypes);
            }
            else if (pdbType is PointerRecord pointerRecord)
            {
                Assert.Equal(diaSymbol.length, pointerRecord.Size);
                Assert.Equal(diaSymbol.constType, pointerRecord.IsConst);
                Assert.Equal(diaSymbol.unalignedType, pointerRecord.IsUnaligned);
                Assert.Equal(diaSymbol.volatileType, pointerRecord.IsVolatile);
                CompareTypes(diaSymbol.type, pdb, pointerRecord.ReferentType, checkedTypes);
            }
            else if (pdbType is BitFieldRecord bitFieldRecord)
            {
                // TODO: Bit wise characteristics needs to be verified. This is part of field and not the type.
                CompareTypes(diaSymbol, pdb, bitFieldRecord.Type, checkedTypes);
            }
            else if (pdbType is ProcedureRecord procedureRecord)
            {
                // TODO: Implement comparing procedures.
            }
            else
            {
                // TODO:
                throw new NotImplementedException();
            }
        }

        private static IEnumerable<TypeRecord> GetFields(PdbFile pdb, TagRecord record)
        {
            foreach (TypeRecord field in EnumerateFieldList(pdb, record.FieldList))
                if (field is DataMemberRecord || field is StaticDataMemberRecord staticDataMember)
                    yield return field;
        }

        private static IEnumerable<TypeRecord> GetBaseClasses(PdbFile pdb, ClassRecord classRecord)
        {
            if (classRecord.DerivationList != TypeIndex.None)
            {
                TypeRecord baseClasses = pdb.TpiStream[classRecord.DerivationList];

                throw new System.NotImplementedException();
            }
            else if (classRecord.FieldList != TypeIndex.None)
            {
                foreach (TypeRecord field in EnumerateFieldList(pdb, classRecord.FieldList))
                    if (field is BaseClassRecord || field is VirtualBaseClassRecord)
                        yield return field;
            }
        }

        private static IEnumerable<TypeRecord> EnumerateFieldList(PdbFile pdb, TypeIndex fieldListIndex)
        {
            while (fieldListIndex != TypeIndex.None)
            {
                TypeIndex nextFieldListIndex = TypeIndex.None;
                TypeRecord fieldList = pdb.TpiStream[fieldListIndex];
                if (fieldList is FieldListRecord fieldListRecord)
                    foreach (TypeRecord field in fieldListRecord.Fields)
                        if (field is ListContinuationRecord listContinuation)
                            nextFieldListIndex = listContinuation.ContinuationIndex;
                        else
                            yield return field;
                fieldListIndex = nextFieldListIndex;
            }
        }
    }

    public enum UdtKind
    {
        Struct,
        Class,
        Union,
        Interface
    }

    public static class DiaHelpers
    {
        public static IEnumerable<IDiaSymbol> Enum(this IDiaEnumSymbols container)
        {
            IDiaSymbol[] tempSymbols = new IDiaSymbol[1];
            container.Reset();
            while (true)
            {
                uint count;

                container.Next((uint)tempSymbols.Length, tempSymbols, out count);
                if (count == 0)
                    break;
                yield return tempSymbols[0];
            }
        }

        public static IEnumerable<IDiaSymbol> GetChildren(this IDiaSymbol symbol, SymTagEnum tag = SymTagEnum.Null)
        {
            IDiaEnumSymbols symbols = symbol.findChildren(tag, null, NameSearchOptions.None);

            return symbols.Enum();
        }

        public static bool HasVTable(this IDiaSymbol symbol)
        {
            if (symbol.GetChildren(SymTagEnum.VTable).Any())
                return true;
            foreach (IDiaSymbol baseClass in symbol.GetChildren(SymTagEnum.BaseClass))
                if (baseClass.offset == 0 && baseClass.HasVTable())
                    return true;
            return false;
        }
    }
}
