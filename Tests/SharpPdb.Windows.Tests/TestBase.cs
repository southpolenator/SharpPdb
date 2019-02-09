using System;
using System.IO;
using System.Reflection;

namespace SharpPdb.Windows.Tests
{
    public class TestBase
    {
        public const string DefaultPdbsPath = @"..\..\..\pdbs\";

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

        public static PdbFile OpenPdb(int pdbIndex)
        {
            return new PdbFile(GetPdbPath(pdbIndex));
        }
    }
}
