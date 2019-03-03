# SharpPdb
Collections of classes for reading PDB files, both Windows and Portable PDBs.

## Latest status
[![Windows build status](https://ci.appveyor.com/api/projects/status/huvmqxu9tw4me9w1/branch/master?svg=true)](https://ci.appveyor.com/project/southpolenator/sharppdb/branch/master)
[![Linux build status](https://travis-ci.org/southpolenator/SharpPdb.svg?branch=master)](https://travis-ci.org/southpolenator/SharpPdb)
[![Code coverage](https://img.shields.io/codecov/c/github/southpolenator/SharpPdb.svg)](https://codecov.io/github/southpolenator/SharpPdb)
[![Nuget version](https://img.shields.io/nuget/v/sharppdb.svg?style=flat)](https://www.nuget.org/packages/sharppdb/)

If you want newer version than what is available on nuget.org, you can click on [Latest build](https://ci.appveyor.com/project/southpolenator/sharppdb/branch/master), select Configuration and click on Artifacts. You can also use private nuget feed from [AppVeyor CI builds](https://ci.appveyor.com/nuget/sharppdb-qkch7k8m0st4).

## Building
Use `dotnet` command for building and creating nugets:
```
dotnet build
dotnet pack
```

## Running tests
Before running tests, you need to download input PDBs. Change directory to `pdbs` and run download script:
- On Windows: `download.ps1`
- On Linux: `download.sh`

After that, you can use `dotnet` command for running the tests:
```
dotnet test
```

## Using library
If you want to read managed PDBs, you should use [SharpPdb.Managed.PdbFileReader](Source/SharpPdb/Managed/PdbFileReader.cs) class for opening PDB file and continue with [SharpPdb.Managed.IPdbFile](Source/SharpPdb/Managed/IPdbFile.cs) interface.

For reading Windows PDB file directly without wrapper, you can use [SharpPdb.Windows.PdbFile](Source/Windows/PDBFile.cs) class. Then you can use properties to access PDB streams.
