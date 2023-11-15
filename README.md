# VerifyProjectReferences

VerifyProjectReferences is a tool to verify that project references in a Visual Studio solution file (.sln) are correct.  
It will scan all referenced project files. Every project reference in a project file will be checked that it has a GUID (if required) and that it matches with the GUID in the referenced project.

If these GUIDs don't match MSBuild will ignore the project reference, but Visual Studio won't show an error.  
Note: SDK style .csproj files have no GUIDs in the .csproj file but do have one in the .sln file.
SDK style .csproj files don't need GUIDs in there projects references, but .vcxproj files do need GUIDs to work inside Visual Studio.
In this scenario Visual Studio will show a yellow triangle and the project reference is ignored when building withing Visual Studio.

## Features

* .NET 8.0 App.

## Build Instructions

* git clone
* dotnet build

## Usage Instructions

* Run the tool on a solution file: VerifyProjectReferences.exe MySolution.sln
* Check the output and correct the found issues.
