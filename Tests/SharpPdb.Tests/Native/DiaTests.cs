using DIA;
using SharpPdb.Common.Tests;
using SharpPdb.Native.Types;
using SharpPdb.Windows;
using SharpPdb.Windows.TypeRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SharpPdb.Native.Tests
{
    public class DiaTests : TestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(6)]
        public void TestPdb(int pdbIndex)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return;

            string pdbPath = GetPdbPath(pdbIndex);
            IDiaDataSource dia = DiaLoader.CreateDiaSource();
            IDiaSession diaSession;

            dia.loadDataFromPdb(pdbPath);
            dia.openSession(out diaSession);
            using (PdbFileReader pdb = new PdbFileReader(pdbPath))
            {
                // Verify UDTs
                IReadOnlyList<PdbType> pdbUdts = pdb.UserDefinedTypes;
                IDiaSymbol[] diaUdts = diaSession.globalScope.GetChildren(SymTagEnum.UDT)
                    .Concat(diaSession.globalScope.GetChildren(SymTagEnum.Enum))
                    .OrderBy(s => s.constType ? 1 : 0) // Order so that we move types with ModifierRecord to the back
                    .ThenBy(s => s.unalignedType ? 1 : 0)
                    .ThenBy(s => s.volatileType ? 1 : 0)
                    .ToArray();
                HashSet<Tuple<uint, PdbType>> checkedTypes = new HashSet<Tuple<uint, PdbType>>();
                HashSet<string> checkedUdts = new HashSet<string>();
                Dictionary<string, PdbType> pdbTypesByName = new Dictionary<string, PdbType>();

                foreach (PdbType pdbType in pdbUdts)
                    if (!pdbTypesByName.ContainsKey(pdbType.Name))
                        pdbTypesByName.Add(pdbType.Name, pdbType);

                foreach (IDiaSymbol diaType in diaUdts)
                {
                    string name = diaType.name;

                    if (!checkedUdts.Contains(name))
                    {
                        PdbType pdbType = pdbTypesByName[name];

                        CompareTypes(diaType, pdbType, checkedTypes);
                        checkedUdts.Add(name);
                    }
                }

                // Verify global variables
                IDiaSymbol[] diaGlobalVariables = diaSession.globalScope.GetChildren(SymTagEnum.Data).Where(s => s.locationType == LocationType.Static).ToArray();

                Assert.Equal(diaGlobalVariables.Length, pdb.GlobalVariables.Length);
                for (int i = 0; i < diaGlobalVariables.Length; i++)
                {
                    IDiaSymbol diaGlobalVariable = diaGlobalVariables[i];
                    PdbGlobalVariable pdbGlobalVariable = pdb.GlobalVariables[i];

                    Assert.Equal(diaGlobalVariable.name, pdbGlobalVariable.Name);
                    Assert.Equal(diaGlobalVariable.addressSection, pdbGlobalVariable.Segment);
                    Assert.Equal(diaGlobalVariable.addressOffset, pdbGlobalVariable.Offset);
                    Assert.Equal(diaGlobalVariable.relativeVirtualAddress, pdbGlobalVariable.RelativeVirtualAddress);
                    CompareTypes(diaGlobalVariable.type, pdbGlobalVariable.Type, checkedTypes);
                }
            }
        }

        private static void CompareTypes(IDiaSymbol diaType, PdbType pdbType, HashSet<Tuple<uint, PdbType>> checkedTypes)
        {
            uint diaSymbolId = diaType.symIndexId;
            var checkedTypesTuple = Tuple.Create(diaSymbolId, pdbType);

            if (checkedTypes.Contains(checkedTypesTuple))
                return;
            checkedTypes.Add(checkedTypesTuple);

            Assert.Equal(diaType.length, pdbType.Size);
            Assert.Equal(diaType.constType, pdbType.ModifierOptions.HasFlag(ModifierOptions.Const));
            Assert.Equal(diaType.unalignedType, pdbType.ModifierOptions.HasFlag(ModifierOptions.Unaligned));
            Assert.Equal(diaType.volatileType, pdbType.ModifierOptions.HasFlag(ModifierOptions.Volatile));
            if (diaType.symTag == SymTagEnum.UDT)
            {
                Assert.IsAssignableFrom<PdbUserDefinedType>(pdbType);
                PdbUserDefinedType pdbUdt = (PdbUserDefinedType)pdbType;

                if (pdbType.Name != "<unnamed-tag>")
                    Assert.Equal(diaType.name, pdbType.Name);
                Assert.Equal(diaType.nested, pdbUdt.IsNested);
                Assert.Equal(diaType.scoped, pdbUdt.IsScoped);
                Assert.Equal(diaType.packed, pdbUdt.IsPacked);
                Assert.Equal(diaType.overloadedOperator, pdbUdt.HasOverloadedOperator);
                Assert.Equal(diaType.hasNestedTypes, pdbUdt.ContainsNestedClass);
                Assert.Equal(diaType.hasAssignmentOperator, pdbUdt.HasOverloadedAssignmentOperator);
                Assert.Equal(diaType.constructor, pdbUdt.HasConstructorOrDestructor);

                // Fields
                var diaData = diaType.GetChildren(SymTagEnum.Data).ToArray();
                var diaFields = diaData.Where(d => d.locationType == LocationType.ThisRel || d.locationType == LocationType.BitField).ToArray();
                var pdbFields = pdbUdt.Fields;

                Assert.Equal(diaFields.Length, pdbFields.Count);
                for (int i = 0; i < pdbFields.Count; i++)
                {
                    if (pdbFields[i] is PdbTypeBitField bitField)
                    {
                        Assert.Equal(LocationType.BitField, diaFields[i].locationType);
                        Assert.Equal(diaFields[i].bitPosition, bitField.BitOffset);
                        Assert.Equal(diaFields[i].length, bitField.BitSize);
                    }
                    Assert.Equal(diaFields[i].name, pdbFields[i].Name);
                    Assert.Equal((ulong)diaFields[i].offset, pdbFields[i].Offset);
                    CompareTypes(diaFields[i].type, pdbFields[i].Type, checkedTypes);
                }

                // Constants
                var diaConstants = diaData.Where(d => d.locationType == LocationType.Constant).ToArray();
                var pdbConstants = pdbUdt.StaticFields.OfType<PdbTypeConstant>().ToArray();

                Assert.Equal(diaConstants.Length, pdbConstants.Length);
                for (int i = 0; i < pdbConstants.Length; i++)
                {
                    Assert.Equal(diaConstants[i].name, pdbConstants[i].Name);
                    Assert.Equal(diaConstants[i].value.ToString(), pdbConstants[i].Value.ToString());
                    CompareTypes(diaConstants[i].type, pdbConstants[i].Type, checkedTypes);
                }

                // Thread local storage
                var diaTls = diaData.Where(d => d.locationType == LocationType.TLS).ToArray();
                var pdbTls = pdbUdt.StaticFields.OfType<PdbTypeThreadLocalStorage>().ToArray();

                Assert.Equal(diaTls.Length, pdbTls.Length);
                for (int i = 0; i < pdbTls.Length; i++)
                {
                    Assert.Equal(diaTls[i].name, pdbTls[i].Name);
                    Assert.Equal(diaTls[i].addressSection, pdbTls[i].Segment);
                    Assert.Equal(diaTls[i].addressOffset, pdbTls[i].Offset);
                    CompareTypes(diaTls[i].type, pdbTls[i].Type, checkedTypes);
                }

                // Static fields
                var diaStaticFields = diaData.Where(d => d.locationType == LocationType.Static).ToArray();
                var pdbStaticFields = pdbUdt.StaticFields.Where(sf => !(sf is PdbTypeConstant) && !(sf is PdbTypeThreadLocalStorage)).ToArray();

                Assert.Equal(diaStaticFields.Length, pdbStaticFields.Length);
                for (int i = 0; i < pdbStaticFields.Length; i++)
                {
                    Assert.Equal(diaStaticFields[i].name, pdbStaticFields[i].Name);
                    if (pdbStaticFields[i] is PdbTypeRegularStaticField regularStaticField)
                    {
                        Assert.Equal(diaStaticFields[i].addressSection, regularStaticField.Segment);
                        Assert.Equal(diaStaticFields[i].addressOffset, regularStaticField.Offset);
                        Assert.Equal(diaStaticFields[i].relativeVirtualAddress, regularStaticField.RelativeVirtualAddress);
                    }
                    CompareTypes(diaStaticFields[i].type, pdbStaticFields[i].Type, checkedTypes);
                }
            }
            else if (diaType.symTag == SymTagEnum.Enum)
            {
                Assert.IsAssignableFrom<PdbEnumType>(pdbType);
                PdbEnumType pdbEnumType = (PdbEnumType)pdbType;

                Assert.Equal(diaType.name, pdbType.Name);
                Assert.Equal(diaType.nested, pdbEnumType.IsNested);
                Assert.Equal(diaType.scoped, pdbEnumType.IsScoped);
                Assert.Equal(diaType.packed, pdbEnumType.IsPacked);
                Assert.Equal(diaType.overloadedOperator, pdbEnumType.HasOverloadedOperator);
                Assert.Equal(diaType.hasNestedTypes, pdbEnumType.ContainsNestedClass);
                Assert.Equal(diaType.hasAssignmentOperator, pdbEnumType.HasOverloadedAssignmentOperator);
                Assert.Equal(diaType.constructor, pdbEnumType.HasConstructorOrDestructor);
                CompareBaseType(diaType.type, pdbEnumType.UnderlyingType);
                CompareBaseType(diaType, pdbEnumType.UnderlyingType);

                // Compare enumeration values
                var diaValues = diaType.GetChildren(SymTagEnum.Data).ToArray();

                Assert.Equal(diaValues.Length, pdbEnumType.Values.Count);
                for (int i = 0; i < diaValues.Length; i++)
                {
                    Assert.Equal(DataKind.Constant, diaValues[i].dataKind);
                    Assert.Equal(diaValues[i].name, pdbEnumType.Values[i].Name);
                    Assert.Equal(diaValues[i].value.ToString(), pdbEnumType.Values[i].Value.ToString());
                }
            }
            else if (diaType.symTag == SymTagEnum.BaseType)
            {
                CompareBaseType(diaType, pdbType);
            }
            else if (diaType.symTag == SymTagEnum.ArrayType)
            {
                Assert.IsAssignableFrom<PdbArrayType>(pdbType);
                PdbArrayType pdbArrayType = (PdbArrayType)pdbType;

                Assert.Equal(diaType.count, pdbArrayType.Count);
                CompareTypes(diaType.type, pdbArrayType.ElementType, checkedTypes);
                CompareTypes(diaType.arrayIndexType, pdbArrayType.IndexType, checkedTypes);
            }
            else if (diaType.symTag == SymTagEnum.PointerType)
            {
                Assert.IsAssignableFrom<PdbPointerType>(pdbType);
                PdbPointerType pdbPointerType = (PdbPointerType)pdbType;

                Assert.Equal(diaType.reference, pdbPointerType.IsLValueReference);
                Assert.Equal(diaType.RValueReference, pdbPointerType.IsRValueReference);
                CompareTypes(diaType.type, pdbPointerType.ElementType, checkedTypes);
            }
            else if (diaType.symTag == SymTagEnum.FunctionType)
            {
                PdbType returnType;
                CallingConvention callingConvention;
                ushort parameterCount;
                PdbType[] pdbArguments;

                if (pdbType is PdbFunctionType)
                {
                    Assert.IsAssignableFrom<PdbFunctionType>(pdbType);
                    PdbFunctionType pdbFunctionType = (PdbFunctionType)pdbType;

                    returnType = pdbFunctionType.ReturnType;
                    callingConvention = pdbFunctionType.CallingConvention;
                    parameterCount = pdbFunctionType.ParameterCount;
                    pdbArguments = pdbFunctionType.Arguments;
                }
                else
                {
                    Assert.IsAssignableFrom<PdbMemberFunctionType>(pdbType);
                    PdbMemberFunctionType pdbMemberFunctionType = (PdbMemberFunctionType)pdbType;

                    returnType = pdbMemberFunctionType.ReturnType;
                    callingConvention = pdbMemberFunctionType.CallingConvention;
                    parameterCount = pdbMemberFunctionType.ParameterCount;
                    pdbArguments = pdbMemberFunctionType.Arguments;

                    CompareTypes(diaType.objectPointerType, pdbMemberFunctionType.ThisType, checkedTypes);
                    Assert.Equal(diaType.thisAdjust, pdbMemberFunctionType.ThisPointerAdjustment);
                }

                Assert.Equal(diaType.callingConvention, (uint)callingConvention);
                Assert.Equal(diaType.count, parameterCount);
                CompareTypes(diaType.type, returnType, checkedTypes);

                IDiaSymbol[] diaArguments = diaType.GetChildren(SymTagEnum.FunctionArgType).ToArray();

                Assert.Equal(diaArguments.Length, pdbArguments.Length);
                for (int i = 0; i < diaArguments.Length; i++)
                    CompareTypes(diaArguments[i].type, pdbArguments[i], checkedTypes);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static void CompareBaseType(IDiaSymbol diaType, PdbType pdbType)
        {
            Assert.IsAssignableFrom<PdbSimpleType>(pdbType);
            PdbSimpleType pdbSimpleType = (PdbSimpleType)pdbType;

            Assert.True(pdbSimpleType.TypeIndex.IsSimple);
            switch (pdbSimpleType.TypeIndex.SimpleKind)
            {
                case SimpleTypeKind.None:
                    Assert.True(diaType.baseType == BasicType.NoType);
                    break;
                case SimpleTypeKind.Void:
                    Assert.True(diaType.baseType == BasicType.Void);
                    break;
                case SimpleTypeKind.HResult:
                    Assert.True(diaType.baseType == BasicType.Hresult);
                    break;
                case SimpleTypeKind.UnsignedCharacter:
                    Assert.True(diaType.baseType == BasicType.UInt);
                    break;
                case SimpleTypeKind.NarrowCharacter:
                case SimpleTypeKind.SignedCharacter:
                    Assert.True(diaType.baseType == BasicType.Char || diaType.baseType == BasicType.Int);
                    break;
                case SimpleTypeKind.WideCharacter:
                    Assert.True(diaType.baseType == BasicType.WChar);
                    break;
                case SimpleTypeKind.Character16:
                    Assert.True(diaType.baseType == BasicType.Char16);
                    break;
                case SimpleTypeKind.Character32:
                    Assert.True(diaType.baseType == BasicType.Char32);
                    break;
                case SimpleTypeKind.SByte:
                    Assert.True(diaType.baseType == BasicType.Int);
                    break;
                case SimpleTypeKind.Byte:
                    Assert.True(diaType.baseType == BasicType.UInt);
                    break;
                case SimpleTypeKind.Int16Short:
                case SimpleTypeKind.Int16:
                    Assert.True(diaType.baseType == BasicType.Int);
                    break;
                case SimpleTypeKind.UInt16:
                case SimpleTypeKind.UInt16Short:
                    Assert.True(diaType.baseType == BasicType.UInt);
                    break;
                case SimpleTypeKind.Int32Long:
                case SimpleTypeKind.Int32:
                    Assert.True(diaType.baseType == BasicType.Long || diaType.baseType == BasicType.Int);
                    break;
                case SimpleTypeKind.UInt32Long:
                case SimpleTypeKind.UInt32:
                    Assert.True(diaType.baseType == BasicType.ULong || diaType.baseType == BasicType.UInt);
                    break;
                case SimpleTypeKind.Int64Quad:
                case SimpleTypeKind.Int64:
                    Assert.True(diaType.baseType == BasicType.Long || diaType.baseType == BasicType.Int);
                    break;
                case SimpleTypeKind.UInt64Quad:
                case SimpleTypeKind.UInt64:
                    Assert.True(diaType.baseType == BasicType.ULong || diaType.baseType == BasicType.UInt);
                    break;
                case SimpleTypeKind.Int128Oct:
                case SimpleTypeKind.Int128:
                    Assert.True(diaType.baseType == BasicType.Long || diaType.baseType == BasicType.Int);
                    break;
                case SimpleTypeKind.UInt128Oct:
                case SimpleTypeKind.UInt128:
                    Assert.True(diaType.baseType == BasicType.ULong || diaType.baseType == BasicType.UInt);
                    break;
                case SimpleTypeKind.Float16:
                    Assert.True(diaType.baseType == BasicType.Float);
                    break;
                case SimpleTypeKind.Float32:
                case SimpleTypeKind.Float32PartialPrecision:
                case SimpleTypeKind.Float64:
                case SimpleTypeKind.Float80:
                case SimpleTypeKind.Float128:
                    Assert.True(diaType.baseType == BasicType.Float);
                    break;
                case SimpleTypeKind.Complex16:
                case SimpleTypeKind.Complex32:
                case SimpleTypeKind.Complex32PartialPrecision:
                case SimpleTypeKind.Complex48:
                case SimpleTypeKind.Complex64:
                case SimpleTypeKind.Complex80:
                case SimpleTypeKind.Complex128:
                    Assert.True(diaType.baseType == BasicType.Complex);
                    break;
                case SimpleTypeKind.Boolean8:
                case SimpleTypeKind.Boolean16:
                case SimpleTypeKind.Boolean32:
                case SimpleTypeKind.Boolean64:
                case SimpleTypeKind.Boolean128:
                    Assert.True(diaType.baseType == BasicType.Bool);
                    break;
                case SimpleTypeKind.NotTranslated:
                    Assert.True(diaType.baseType == BasicType.NoType);
                    break;
                default:
                    throw new NotImplementedException($"Unexpected simple type: {pdbType.TypeIndex.SimpleKind}, from type index: {pdbType.TypeIndex}");
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
