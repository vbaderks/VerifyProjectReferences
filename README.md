# VerifyProjectReferences

VerifyProjectReferences is a tool to verify that project references in a Visual Studio solution file (.sln) are correct.  
It will scan all referenced project files. Every project reference in a project file will be checked that it has a GUID (if required) and that it matches with the GUID in the referenced project.

If these GUIDs don't match MSBuild will ignore the project reference, but Visual Studio won't show an error.
Note: Project references to SDK style .csproj files have no GUIDs, this is to be expected and fine. Also SDK style .csproj files don't need GUIDs in there projects references.

## Features

* .NET 6.0 App.

## Build Instructions

* git clone
* dotnet build

## Usage Instructions

* Run the tool on a solution file: VerifyProjectReferences.exe MySolution.sln
* Check the output and correct found isses.
