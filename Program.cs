using Microsoft.Build.Construction;
using System;
using System.IO;
using static System.Console;

[assembly: CLSCompliant(true)]

const int success = 0;
const int failure = 1;
const string projectReferenceItemType = "ProjectReference";

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

                var actualGuid = GetProjectGuid(project.AbsolutePath, item.Include);
                var referenceGuid = GetProjectReferenceGuid(item);
                if (actualGuid != referenceGuid)
                {
                    WriteLine("   Invalid project reference GUID {0} (actual = {1})", referenceGuid, actualGuid);
                }
            }
        }
    }

    return success;
}
catch (Exception e)
{
    WriteLine("Error: " + e.Message);
    return failure;
}

static Guid GetProjectGuid(string absoluteProjectPath, string relativeProjectPath)
{
    string? projectDirectory = Path.GetDirectoryName(absoluteProjectPath);
    if (projectDirectory == null)
        return Guid.Empty;

    string referencedProjectPath = Path.Combine(projectDirectory, relativeProjectPath);
    var msbuildProject = ProjectRootElement.Open(referencedProjectPath);
    if (msbuildProject == null)
        return Guid.Empty;

    foreach (var prop in msbuildProject.Properties)
    {
        if (prop.Name == "ProjectGuid")
            return new Guid(prop.Value);
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
