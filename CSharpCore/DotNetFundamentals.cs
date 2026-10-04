using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace CSharpCore;

/// <summary>
/// Lesson 9 and 10: .NET runtime concepts, assemblies, the Base Class
/// Library, the Garbage Collector, and common .NET CLI commands.
/// </summary>
public static class DotNetFundamentals
{
    public static string GetFrameworkDescription()
    {
        // RuntimeInformation reports the runtime executing this application.
        // The same application can run on different operating systems.
        return RuntimeInformation.FrameworkDescription;
    }

    public static string GetProcessArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture.ToString();
    }

    public static string GetAssemblyIdentity()
    {
        // An assembly is the compiled deployment unit that contains types and
        // metadata. A project commonly produces an assembly such as a DLL.
        Assembly assembly = typeof(DotNetFundamentals).Assembly;
        AssemblyName assemblyName = assembly.GetName();
        string name = assemblyName.Name ?? "Unknown assembly";
        string version = assemblyName.Version?.ToString() ?? "Unknown version";
        return $"{name} {version}";
    }

    public static string GetBaseClassLibraryIdentity()
    {
        // System.Object lives in the Base Class Library (BCL). The BCL gives
        // every .NET project common types such as String, List, and Task.
        Assembly baseClassLibrary = typeof(object).Assembly;
        return baseClassLibrary.GetName().Name ?? "Unknown BCL";
    }

    public static long GetManagedMemoryBytes(bool forceFullCollection = false)
    {
        // The Garbage Collector manages memory for managed objects. This is a
        // measurement for learning, not a stable application metric.
        return GC.GetTotalMemory(forceFullCollection);
    }

    public static bool IsServerGarbageCollectionEnabled()
    {
        return GCSettings.IsServerGC;
    }

    public static IReadOnlyList<string> GetCommonCliCommands()
    {
        // These are the commands used to inspect, create, build, test, and
        // publish .NET projects from a terminal.
        return new[]
        {
            "dotnet --info",
            "dotnet new console",
            "dotnet restore",
            "dotnet build",
            "dotnet run",
            "dotnet test",
            "dotnet add package <package-name>",
            "dotnet publish"
        };
    }

    public static IReadOnlyList<string> GetProjectArtifacts()
    {
        // A project file describes how the SDK builds the project. bin and obj
        // are generated folders and should normally stay out of source control.
        return new[]
        {
            ".csproj",
            "Program.cs",
            "appsettings.json",
            "bin/",
            "obj/",
            "NuGet package references"
        };
    }
}
