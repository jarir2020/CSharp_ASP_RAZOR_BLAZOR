# Build with the .NET SDK image and run with the smaller ASP.NET Core image.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ProductionLab/ProductionLab.csproj ProductionLab/
RUN dotnet restore ProductionLab/ProductionLab.csproj

COPY ProductionLab/ ProductionLab/
RUN dotnet publish ProductionLab/ProductionLab.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# APP_UID is supplied by the official .NET runtime image. Running without
# root privileges reduces the impact of a compromised application process.
USER $APP_UID
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProductionLab.dll"]
