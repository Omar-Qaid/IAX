using System.Xml.Linq;

namespace IAX.IXMcp.Tests;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void Server_project_has_only_the_MCP_runtime_package()
    {
        var repositoryRoot = FindRepositoryRoot();
        var project = XDocument.Load(Path.Combine(repositoryRoot, "IXMcp", "IXMcp.csproj"));
        var packageNames = project
            .Descendants("PackageReference")
            .Select(reference => (string?)reference.Attribute("Include"))
            .Where(name => name is not null)
            .Select(name => name!)
            .ToArray();
        var projectReferences = project.Descendants("ProjectReference").ToArray();

        Assert.Equal(["ModelContextProtocol.AspNetCore"], packageNames);
        Assert.Empty(projectReferences);
    }

    [Fact]
    public void Server_and_tests_pin_the_same_SDK()
    {
        var repositoryRoot = FindRepositoryRoot();
        var serverPin = File.ReadAllText(Path.Combine(repositoryRoot, "IXMcp", "global.json"));
        var testPin = File.ReadAllText(Path.Combine(repositoryRoot, "IXMcp.Tests", "global.json"));

        Assert.Equal(serverPin, testPin);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
