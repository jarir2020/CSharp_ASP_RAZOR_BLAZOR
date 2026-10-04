using CSharpCore;
using Xunit;

namespace CSharpCore.Tests;

public class DotNetFundamentalsTests
{
    [Fact]
    public void Runtime_reports_a_dotnet_framework_description()
    {
        string framework = DotNetFundamentals.GetFrameworkDescription();

        Assert.True(framework.Contains(".NET", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Compiled_code_has_an_assembly_identity_and_a_base_class_library()
    {
        Assert.Contains("CSharpCore", DotNetFundamentals.GetAssemblyIdentity());
        Assert.NotEqual("Unknown BCL", DotNetFundamentals.GetBaseClassLibraryIdentity());
    }

    [Fact]
    public void Cli_lesson_lists_the_core_project_commands()
    {
        IReadOnlyList<string> commands = DotNetFundamentals.GetCommonCliCommands();

        Assert.Contains("dotnet build", commands);
        Assert.Contains("dotnet test", commands);
        Assert.Contains("dotnet publish", commands);
    }

    [Fact]
    public void Project_lesson_names_source_and_generated_artifacts()
    {
        IReadOnlyList<string> artifacts = DotNetFundamentals.GetProjectArtifacts();

        Assert.Contains(".csproj", artifacts);
        Assert.Contains("bin/", artifacts);
        Assert.Contains("obj/", artifacts);
    }
}
