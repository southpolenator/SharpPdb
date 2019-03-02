using SharpPdb.Windows.DebugSubsections;
using SharpPdb.Windows.SymbolRecords;
using SharpUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SharpPdb.Windows.Tests.E2E
{
    public class ClrMdParseTests : TestBase
    {
        [Fact]
        public void Test2()
        {
            using (PdbFile pdb = OpenPdb(2))
            {
                // Verify info stream header
                Assert.Equal(PIS.InfoStreamVersion.VC70, pdb.InfoStream.Header.Version);
                Assert.Equal(0xC152E0B3U, pdb.InfoStream.Header.Signature);
                Assert.Equal(1U, pdb.InfoStream.Header.Age);
                Assert.Equal(Guid.Parse("{483548d2-af63-4b4e-bce0-41e1bd07d062}"), pdb.InfoStream.Header.Guid);

                // Verify names stream map
                var namesStreamMap = pdb.InfoStream.NamedStreamMap.Streams;
                Assert.Equal(4, namesStreamMap.Count);
                Assert.Equal(7, namesStreamMap["/src/headerblock"]);
                Assert.Equal(6, namesStreamMap["/names"]);
                Assert.Equal(5, namesStreamMap["/LinkInfo"]);
                Assert.Equal(9, namesStreamMap[@"/src/files/c:\projects\windbgcs-dumps\source\clr\sharedlibrary\sharedlibrary.cs"]);

                // Verify names stream
                PdbStringTable namesStream = new PdbStringTable(pdb.Streams[pdb.InfoStream.NamedStreamMap.Streams["/names"]].Reader);

                Assert.Equal(@"C:\projects\windbgcs-dumps\Source\Clr\SharedLibrary\SharedLibrary.cs", namesStream.Dictionary[1]);
                Assert.Equal(@"c:\projects\windbgcs-dumps\source\clr\sharedlibrary\sharedlibrary.cs", namesStream.Dictionary[71]);

                // Verify DBI modules
                var dbiModules = pdb.DbiStream.Modules;

                Assert.Equal(4, dbiModules.Count);

                // Verify DBI module 0
                var dbiModule = dbiModules[0];
                Assert.Equal("Foo", dbiModule.ModuleName);
                var localss = dbiModule.LocalSymbolStream;
                var functions = GetManagedProcedures(localss);
                Assert.Equal(4, functions.Length);
                Assert.Equal("Bar", functions[0].Name);
                Assert.Equal(new byte[] { 0 }, ReadCustomMetadata(functions[0]));
                Assert.Equal("Baz", functions[1].Name);
                Assert.Equal(new byte[] { 1 }, ReadCustomMetadata(functions[1]));
                Assert.Equal("Baz", functions[2].Name);
                Assert.Equal(new byte[] { 1, 6 }, ReadCustomMetadata(functions[2]));
                Assert.Equal(".ctor", functions[3].Name);
                Assert.Equal(new byte[] { 1 }, ReadCustomMetadata(functions[3]));
                var checksums = dbiModule.DebugSubsectionStream[DebugSubsectionKind.FileChecksums].OfType<FileChecksumSubsection>().ToArray();
                Assert.Equal(@"C:\projects\windbgcs-dumps\Source\Clr\SharedLibrary\SharedLibrary.cs", namesStream.Dictionary[checksums[0].NameIndex]);
                Assert.Single(checksums);
                var guidStream = GetGuidStream(checksums[0], namesStream, pdb);
                Assert.Equal(new byte[] { 248, 0, 227, 166, 171, 152, 157, 122, 60, 185, 89, 14, 74, 143, 196, 154, 218, 146, 222, 2 }, guidStream.ChecksumReader.ReadAllBytes());
                var lines = dbiModule.DebugSubsectionStream[DebugSubsectionKind.Lines].OfType<LinesSubsection>().ToArray();
                Assert.Equal(4, lines.Length);
                Assert.Equal(2 + 2 + 3 + 16, lines.SelectMany(l => l.Files).SelectMany(f => f.Lines).Count());

                // Verify DBI module 1
                dbiModule = dbiModules[1];
                Assert.Equal("Struct", dbiModule.ModuleName);
                localss = dbiModule.LocalSymbolStream;
                functions = GetManagedProcedures(localss);
                Assert.Single(functions);
                Assert.Equal(".ctor", functions[0].Name);
                Assert.Equal(new byte[] { 1 }, ReadCustomMetadata(functions[0]));
                checksums = dbiModule.DebugSubsectionStream[DebugSubsectionKind.FileChecksums].OfType<FileChecksumSubsection>().ToArray();
                Assert.Equal(1U, checksums[0].NameIndex);
                lines = dbiModule.DebugSubsectionStream[DebugSubsectionKind.Lines].OfType<LinesSubsection>().ToArray();
                Assert.Single(lines);
                Assert.Equal(9, lines.SelectMany(l => l.Files).SelectMany(f => f.Lines).Count());

                // Verify DBI module 2
                dbiModule = dbiModules[2];
                Assert.Equal("MiddleStruct", dbiModule.ModuleName);
                localss = dbiModule.LocalSymbolStream;
                functions = GetManagedProcedures(localss);
                Assert.Single(functions);
                Assert.Equal(".ctor", functions[0].Name);
                Assert.Equal(new byte[] { 1 }, ReadCustomMetadata(functions[0]));
                checksums = dbiModule.DebugSubsectionStream[DebugSubsectionKind.FileChecksums].OfType<FileChecksumSubsection>().ToArray();
                Assert.Equal(1U, checksums[0].NameIndex);
                lines = dbiModule.DebugSubsectionStream[DebugSubsectionKind.Lines].OfType<LinesSubsection>().ToArray();
                Assert.Single(lines);
                Assert.Equal(9, lines.SelectMany(l => l.Files).SelectMany(f => f.Lines).Count());

                // Verify DBI module 3
                dbiModule = dbiModules[3];
                Assert.Equal("InnerStruct", dbiModule.ModuleName);
                localss = dbiModule.LocalSymbolStream;
                functions = GetManagedProcedures(localss);
                Assert.Single(functions);
                Assert.Equal(".ctor", functions[0].Name);
                Assert.Equal(new byte[] { 1 }, ReadCustomMetadata(functions[0]));
                checksums = dbiModule.DebugSubsectionStream[DebugSubsectionKind.FileChecksums].OfType<FileChecksumSubsection>().ToArray();
                Assert.Equal(1U, checksums[0].NameIndex);
                lines = dbiModule.DebugSubsectionStream[DebugSubsectionKind.Lines].OfType<LinesSubsection>().ToArray();
                Assert.Single(lines);
                Assert.Equal(8, lines.SelectMany(l => l.Files).SelectMany(f => f.Lines).Count());

                // Verify functions token
                functions = dbiModules.SelectMany(d => GetManagedProcedures(d.LocalSymbolStream)).ToArray();
                Assert.Equal(new uint[] { 0x06000001, 0x06000002, 0x06000003, 0x06000004, 0x06000005, 0x06000006, 0x06000007 }, functions.Select(f => f.FunctionType.Index));
            }
        }

        private GuidStream GetGuidStream(FileChecksumSubsection checksum, PdbStringTable namesStream, PdbFile pdb)
        {
            string name = namesStream.Dictionary[checksum.NameIndex];
            int guidStreamIndex;

            if (pdb.InfoStream.NamedStreamMap.Streams.TryGetValue("/src/files/" + name, out guidStreamIndex)
                || pdb.InfoStream.NamedStreamMap.StreamsUppercase.TryGetValue("/SRC/FILES/" + name.ToUpperInvariant(), out guidStreamIndex))
            {
                return new GuidStream(pdb.Streams[guidStreamIndex]);
            }
            return null;
        }

        private ManagedProcedureSymbol[] GetManagedProcedures(SymbolStream symbolStream)
        {
            List<ManagedProcedureSymbol> managedProcedures = new List<ManagedProcedureSymbol>();

            foreach (var kind in ManagedProcedureSymbol.Kinds)
                managedProcedures.AddRange(symbolStream[kind].OfType<ManagedProcedureSymbol>());
            return managedProcedures.ToArray();
        }

        private static readonly Guid MsilMetadataGuid = Guid.Parse("{c6ea3fc9-59b3-49d6-bc25-0902bbabb460}");

        private IEnumerable<byte> ReadCustomMetadata(ManagedProcedureSymbol function)
        {
            foreach (OemSymbol oem in function.Children.OfType<OemSymbol>())
                if (oem.Id == MsilMetadataGuid)
                {
                    IBinaryReader reader = oem.UserDataReader;
                    string name = reader.ReadCStringWide();

                    if (name == "MD2")
                    {
                        byte version = reader.ReadByte();

                        if (version == 4)
                        {
                            int count = reader.ReadByte();

                            reader.Align(4);
                            for (int i = 0; i < count; i++)
                            {
                                long start = reader.Position;
                                byte entryVersion = reader.ReadByte();
                                byte kind = reader.ReadByte();

                                reader.Align(4);
                                yield return kind;
                                uint numberOfBytesInItem = reader.ReadUint();
                                reader.Position = start + numberOfBytesInItem;
                            }
                        }
                    }
                }
        }
    }
}
