
# Convert Project Dependencies to MSBuild Project references

ConvertProjDepToProjRef is a small tool to convert project dependencies in a Visual Studio solution file (.sln)
into MSBuild project references (ProjectReference) in project files. It will do 95% of the work.

## Features

* .NET 5.0 App.

## Build Instructions

* git clone
* dotnet build

## Usage Instructions

* Run the tool on a solution file: ConvertProjDepToProjRef.exe MySolution.sln
* Update C++ projects that only perform custom builds as ConfigurationType “Utility”
* Use a text editor to remove the ProjectSection(ProjectDependencies) = postProject sections from the .sln file.
