using SharpPdb.Common.Tests;
using System;
using System.Linq;
using Xunit;

namespace SharpPdb.Managed.Tests
{
    public class WindowsPdbVerificationTests : TestBase
    {
        [Fact]
        public void Test2()
        {
            using (IPdbFile pdb = OpenManagedPdb(2))
            {
                Assert.IsType<Windows.PdbFile>(pdb);
                Assert.Equal(1, pdb.Age);
                Assert.Equal(Guid.Parse("483548d2-af63-4b4e-bce0-41e1bd07d062"), pdb.Guid);
                Assert.Equal(3243434163U, pdb.Stamp);

                Assert.Equal(7, pdb.Functions.Count);
                IPdbFunction function = pdb.Functions[0];

                Assert.Empty(function.LocalScopes);
                Assert.Equal(100663297, function.Token);
                Assert.Equal(function, pdb.GetFunctionFromToken(function.Token));
                Assert.Equal(23, function.SequencePoints.Count);

                IPdbSequencePoint sequencePoint = function.SequencePoints[0];
                Assert.Equal(0, sequencePoint.Offset);
                Assert.Equal(22, sequencePoint.StartLine);
                Assert.Equal(22, sequencePoint.EndLine);
                Assert.Equal(23, sequencePoint.StartColumn);
                Assert.Equal(24, sequencePoint.EndColumn);

                var sources = pdb.Functions.SelectMany(f => f.SequencePoints).Select(sp => sp.Source).GroupBy(s => s.Name).Select(sg => sg.First());
                Assert.Single(sources);
                IPdbSource source = sources.First();
                Assert.Equal(@"C:\projects\windbgcs-dumps\Source\Clr\SharedLibrary\SharedLibrary.cs", source.Name);
                Assert.Equal(Guid.Parse("3f5162f8-07c6-11d3-9053-00c04fa302a1"), source.Language);
                Assert.Equal(Guid.Parse("ff1816ec-aa5e-4d10-87f7-6f4963833460"), source.HashAlgorithm);
                Assert.Equal(new byte[] { 248, 0, 227, 166, 171, 152, 157, 122, 60, 185, 89, 14, 74, 143, 196, 154, 218, 146, 222, 2 }, source.Hash);
            }
        }

        [Fact]
        public void Test3()
        {
            using (IPdbFile pdb = OpenManagedPdb(3))
            {
                Assert.IsType<Windows.PdbFile>(pdb);
                Assert.Equal(1, pdb.Age);
                Assert.Equal(Guid.Parse("6a15f24f-b0ee-400f-9136-7950178b42f5"), pdb.Guid);
                Assert.Equal(2499347773U, pdb.Stamp);

                Assert.Equal(6, pdb.Functions.Count);
                IPdbFunction function = pdb.Functions[0];

                Assert.Single(function.LocalScopes);
                Assert.Equal(100663297, function.Token);
                Assert.Equal(function, pdb.GetFunctionFromToken(function.Token));
                Assert.Equal(1, function.SequencePoints.Count);

                IPdbLocalScope localScope = function.LocalScopes[0];
                Assert.Equal(0, localScope.StartOffset);
                Assert.Equal(19, localScope.EndOffset);
                Assert.Equal(localScope.EndOffset - localScope.StartOffset, localScope.Length);
                Assert.Equal(function, localScope.Function);
                Assert.Null(localScope.Parent);
                Assert.Empty(localScope.Children);
                Assert.Empty(localScope.Constants);
                Assert.Empty(localScope.Variables);

                IPdbSequencePoint sequencePoint = function.SequencePoints[0];
                Assert.Equal(0, sequencePoint.Offset);
                Assert.Equal(8, sequencePoint.StartLine);
                Assert.Equal(8, sequencePoint.EndLine);
                Assert.Equal(5, sequencePoint.StartColumn);
                Assert.Equal(55, sequencePoint.EndColumn);

                var sources = pdb.Functions.SelectMany(f => f.SequencePoints).Select(sp => sp.Source).GroupBy(s => s.Name).Select(sg => sg.First());
                Assert.Single(sources);
                IPdbSource source = sources.First();
                Assert.Equal(@"C:\projects\windbgcs-dumps\Source\Clr\LocalVariables\LocalVariables.cs", source.Name);
                Assert.Equal(Guid.Parse("3f5162f8-07c6-11d3-9053-00c04fa302a1"), source.Language);
                Assert.Equal(Guid.Parse("ff1816ec-aa5e-4d10-87f7-6f4963833460"), source.HashAlgorithm);
                Assert.Equal(new byte[] { 150, 161, 4, 5, 30, 51, 251, 92, 205, 46, 22, 14, 23, 237, 168, 175, 33, 178, 190, 218 }, source.Hash);
            }
        }
    }
}
