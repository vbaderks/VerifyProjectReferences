
# Verify project references

VerifyProjectReferences is a small tool to verify project references in a Visual Studio solution file (.sln)
It will scan all included project files and check that the GUIDS in the project reference are matching with
the actual project GUID. If these Guids don't match MSBuild won't build the depedencies correctly.
Note: Project refernces to SDK style .csproj files will have no GUIDs, this is to be expected and fine.

## Features

* .NET 5.0 App.

## Build Instructions

* git clone
* dotnet build

## Usage Instructions

* Run the tool on a solution file: VerifyProjectReferences.exe MySolution.sln
* Check the output and correct found isses.
