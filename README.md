# SharpPdb
Collections of classes for reading PDB files, both Windows and Portable PDBs.

## Latest status
[![Windows build status](https://ci.appveyor.com/api/projects/status/huvmqxu9tw4me9w1/branch/master?svg=true)](https://ci.appveyor.com/project/southpolenator/sharppdb/branch/master)
[![Linux build status](https://travis-ci.org/southpolenator/SharpPdb.svg?branch=master)](https://travis-ci.org/southpolenator/SharpPdb)
[![Code coverage](https://img.shields.io/codecov/c/github/southpolenator/SharpPdb.svg)](https://codecov.io/github/southpolenator/SharpPdb)
[![Nuget version](https://img.shields.io/nuget/v/sharppdb.svg?style=flat)](https://www.nuget.org/packages/sharppdb/)

If you want newer version than what is available on nuget.org, you can click on [Latest build](https://ci.appveyor.com/project/southpolenator/sharppdb/branch/master), select Configuration and click on Artifacts. You can also use private nuget feed from [AppVeyor CI builds](https://ci.appveyor.com/nuget/sharppdb-qkch7k8m0st4).

## Building and testing
Use `dotnet` for building, creating nuget and running tests:
```
dotnet build
dotnet pack
dotnet test
```
