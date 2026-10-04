# gRPC companion exercise

The Phase 13 web lab includes the
[course.proto](../Protos/course.proto) service contract. gRPC code generation
is package-backed, so this small companion exercise keeps the main lab
buildable with the shared ASP.NET Core framework while showing the exact
additions used by a real gRPC project.

Add these package references to a separate GrpcLab.csproj:

~~~xml
<PackageReference Include="Grpc.AspNetCore" Version="2.71.0" />
<PackageReference Include="Grpc.Tools" Version="2.71.0">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
~~~

Then include the contract:

~~~xml
<Protobuf Include="../AdvancedWebLab/Protos/course.proto" GrpcServices="Server" />
~~~

The generated base class is implemented by a server type:

~~~csharp
public sealed class CourseCatalogGrpcService : CourseCatalog.CourseCatalogBase
{
    public override Task<CourseReply> GetCourse(
        CourseRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new CourseReply
        {
            Id = request.Id,
            Title = "C# Fundamentals",
            Level = "Beginner"
        });
    }
}
~~~

Register the framework and map the generated service:

~~~csharp
builder.Services.AddGrpc();
app.MapGrpcService<CourseCatalogGrpcService>();
~~~

The proto file is the contract. The generated C# types, HTTP/2 transport, and
package references are the implementation layer. Keep this separation in mind
when comparing gRPC with JSON HTTP endpoints.
