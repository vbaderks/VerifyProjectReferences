// Copyright (c) Victor Derks.
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
    WriteLine("Usage: VerifyProjectReferences solution-filename.sln");
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
    WriteLine("Verifying solution {0}", solutionPath);

    foreach (var project in solutionFile.ProjectsInOrder)
    {
        if (project.ProjectType != SolutionProjectType.KnownToBeMSBuildFormat)
            continue;

        WriteLine(" Verifying project {0} ({1})", project.ProjectName, project.RelativePath);
        var msbuildProject = ProjectRootElement.Open(project.AbsolutePath);
        if (msbuildProject == null)
            throw new FileLoadException();

        foreach (var item in msbuildProject.Items)
        {
            if (item.ItemType == projectReferenceItemType)
            {
                WriteLine("  Verifying ProjectReference {0} ", item.Include);

                try
                {
                    var referenceGuid = GetProjectReferenceGuid(item);
                    if (IsDotNetSdkProject(project.AbsolutePath))
                    {
                        if (!referenceGuid.Equals(Guid.Empty))
                        {
                            DisplayErrorMessage("   Not needed project reference GUID {0}", referenceGuid);
                            errorsFound++;
                        }
                    }
                    else
                    {
                        var projectGuid = GetProjectGuidOfReference(project.AbsolutePath, item.Include, solutionFile);

                        if (projectGuid != referenceGuid)
                        {
                            DisplayErrorMessage("   Invalid project reference GUID {0} (actual = {1})", referenceGuid, projectGuid);
                            errorsFound++;
                        }
                    }
                }
                catch (Exception e) when (e is InvalidProjectFileException or IOException)
                {
                    DisplayErrorMessage("   {0}", e.Message);
                    errorsFound++;
                }
            }
        }
    }

    if (errorsFound != 0)
    {
        DisplayErrorMessage("\nErrors detected: error count = {0}", errorsFound);
        return failure;
    }

    return success;
}
catch (Exception e)
{
    DisplayErrorMessage("Error: {0}", e.Message);
    return failure;
}

static Guid GetProjectGuidOfReference(string absoluteProjectPath, string relativeProjectPath, SolutionFile solutionFile)
{
    string? projectDirectory = Path.GetDirectoryName(absoluteProjectPath);
    if (projectDirectory == null)
        throw new IOException($"Failed to get directory name from {absoluteProjectPath}");

    string referencedProjectPath = Path.Combine(projectDirectory, relativeProjectPath);
    var msbuildProject = ProjectRootElement.Open(referencedProjectPath);
    if (msbuildProject == null)
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
        if (child is ProjectMetadataElement {Name: "Project"} metadata)
            return new Guid(metadata.Value);
    }

    return Guid.Empty;
}

static bool IsDotNetSdkProject(string projectPath)
{
    if (Path.GetExtension(projectPath) != ".csproj")
        return false;

    var project = ProjectRootElement.Open(projectPath);
    return !string.IsNullOrEmpty(project.Sdk);
}

static void DisplayErrorMessage(string format, params object[] arguments)
{
    ForegroundColor = ConsoleColor.Red;
    WriteLine(format, arguments);
    ResetColor();
}
