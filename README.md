<!--
  SPDX-FileCopyrightText: © 2021 Victor Derks
  SPDX-License-Identifier: MIT
-->

# VerifyProjectReferences

VerifyProjectReferences is a tool to verify that project references in a Visual Studio solution file (.sln) are correct.  
It can be used to find the root cause of the yellow triangles that are shown in the Visual Studio dependency list of a project.
It is useful for solutions with mixed C++ and C# projects.

The tool will scan all referenced project files. Every project reference in a project file will be checked that it has
a GUID (if required) and that it matches with the GUID in the referenced project.
If these GUIDs don't match MSBuild will ignore the project reference, but Visual Studio won't show an error.  
Note: SDK style .csproj files have no GUIDs in the .csproj file but do have one in the .sln file.
SDK style .csproj files don't need GUIDs in there projects references, but .vcxproj files do need GUIDs to work inside Visual Studio.
In this scenario Visual Studio will show a yellow triangle and the project reference is ignored when building withing Visual Studio.

## Features

* .NET 9.0 App.

## Build Instructions

Open a console windows and execute the following commands:

```bash
git clone https://github.com/vbaderks/VerifyProjectReferences.git
```

```bash
cd VerifyProjectReferences
```

```bash
dotnet build -c Release
```

## Usage Instructions

* Run the tool on a solution file: VerifyProjectReferences.exe MySolution.sln
* Check the output and correct the found issues.
