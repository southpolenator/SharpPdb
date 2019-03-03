using System;
using System.IO;
using System.Reflection;

namespace SharpPdb.Common.Tests
{
    public class TestBase
    {
        public static readonly string DefaultPdbsPath = Path.Combine("..", "..", "..", "pdbs");

        public static Assembly GetTestsAssembly()
        {
            return typeof(TestBase).GetTypeInfo().Assembly;
        }

        public static string GetBinFolder()
        {
            Assembly assembly = GetTestsAssembly();
            Uri codeBaseUrl = new Uri(assembly.CodeBase);
            string codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);

            return Path.GetDirectoryName(codeBasePath);
        }

        public static string GetAbsoluteBinPath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(GetBinFolder(), path));
            }

            return path;
        }

        public static string GetPdbPath(int pdbIndex)
        {
            string pdbName = $"{pdbIndex}.pdb";

            return GetAbsoluteBinPath(Path.Combine(DefaultPdbsPath, pdbName));
        }

        public static Windows.PdbFile OpenWindowsPdb(int pdbIndex)
        {
            return new Windows.PdbFile(GetPdbPath(pdbIndex));
        }

        public static Managed.IPdbFile OpenManagedPdb(int pdbIndex)
        {
            return Managed.PdbFileReader.OpenPdb(GetPdbPath(pdbIndex));
        }
    }
}
