// SPDX-FileCopyrightText: © 2021 Victor Derks
// SPDX-License-Identifier: MIT

using Microsoft.Build.Construction;
using Microsoft.Build.Exceptions;
using static System.Console;

const int success = 0;
const int failure = 1;
const string projectReferenceItemType = "ProjectReference";
int errorsFound = 0;

if (args.Length < 1)
{
    WriteLine("Usage: VerifyProjectReferences <solution-filename.sln>");
    return failure;
}

try
{
    var solutionPath = args[0];
    if (!Path.IsPathRooted(solutionPath))
    {
        solutionPath = Path.GetFullPath(solutionPath, Directory.GetCurrentDirectory());
    }

    var solutionFile = SolutionFile.Parse(solutionPath);
    WriteLine($"Verifying solution {solutionPath}");

    foreach (var project in solutionFile.ProjectsInOrder)
    {
        if (project.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
            continue;

        WriteLine($" Verifying project {project.ProjectName} ({project.RelativePath})");
        var msbuildProject = ProjectRootElement.Open(project.AbsolutePath) ?? throw new FileLoadException();

        foreach (var item in msbuildProject.Items)
        {
            if (item.ItemType == projectReferenceItemType)
            {
                WriteLine($"  Verifying ProjectReference {item.Include} ");

                try
                {
                    var referenceGuid = GetProjectReferenceGuid(item);
                    if (IsDotNetSdkProject(project.AbsolutePath))
                    {
                        if (!referenceGuid.Equals(Guid.Empty))
                        {
                            DisplayErrorMessage($"   Not needed project reference GUID {referenceGuid}");
                            errorsFound++;
                        }
                    }
                    else
                    {
                        var projectGuid = GetProjectGuidOfReference(project.AbsolutePath, item.Include, solutionFile);

                        if (projectGuid != referenceGuid)
                        {
                            DisplayErrorMessage($"   Invalid project reference GUID {referenceGuid} (actual = {projectGuid})");
                            errorsFound++;
                        }
                    }
                }
                catch (Exception e) when (e is InvalidProjectFileException or IOException)
                {
                    DisplayErrorMessage($"   {e.Message}");
                    errorsFound++;
                }
            }
        }
    }

    if (errorsFound != 0)
    {
        DisplayErrorMessage($"\nErrors detected: error count = {errorsFound}");
        return failure;
    }

    WriteLine("No errors found");
    return success;
}
catch (Exception e)
{
    DisplayErrorMessage($"Error: {e.Message}");
    return failure;
}

static Guid GetProjectGuidOfReference(string absoluteProjectPath, string relativeProjectPath, SolutionFile solutionFile)
{
    string projectDirectory = Path.GetDirectoryName(absoluteProjectPath) ??
                              throw new IOException($"Failed to get directory name from {absoluteProjectPath}");
    string referencedProjectPath = Path.Combine(projectDirectory, relativeProjectPath);
    var msbuildProject = ProjectRootElement.Open(referencedProjectPath) ??
        throw new IOException($"Failed to open {referencedProjectPath}");

    if (IsDotNetSdkProject(referencedProjectPath))
    {
        // .NET SDK style project files have only a GUID in the .sln file.
        foreach (var keyValuePair in solutionFile.ProjectsByGuid)
        {
            string normalizedPath = Path.GetFullPath(referencedProjectPath);

            if (string.Equals(keyValuePair.Value.AbsolutePath, normalizedPath, StringComparison.OrdinalIgnoreCase))
                return new Guid(keyValuePair.Key);
        }
    }
    else
    {
        foreach (var prop in msbuildProject.Properties)
        {
            if (prop.Name == "ProjectGuid")
                return new Guid(prop.Value);
        }
    }

    return Guid.Empty;
}

static Guid GetProjectReferenceGuid(ProjectElementContainer item)
{
    foreach (ProjectElement child in item.Children)
    {
        if (child is ProjectMetadataElement { Name: "Project" } metadata)
            return new Guid(metadata.Value);
    }

    return Guid.Empty;
}

static bool IsDotNetSdkProject(string projectPath)
{
    if (Path.GetExtension(projectPath) != ".csproj")
        return false;

    var project = ProjectRootElement.Open(projectPath);

    // If Sdk property is set, then the .csproj is in the new format. But it can also import the Sdk.
    // If the DefaultTargets="Build" then it is typical an old format.
    return !string.IsNullOrEmpty(project!.Sdk) || string.IsNullOrEmpty(project.DefaultTargets);
}

static void DisplayErrorMessage(string message)
{
    ForegroundColor = ConsoleColor.Red;
    WriteLine(message);
    ResetColor();
}
