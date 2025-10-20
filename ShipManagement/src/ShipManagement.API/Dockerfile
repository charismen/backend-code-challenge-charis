# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ../../backend-code-challenge.sln .
COPY ../../ShipManagement/src/ShipManagement.API/ShipManagement.API.csproj ShipManagement/src/ShipManagement.API/
COPY ../../ShipManagement/tests/ShipManagement.Tests/ShipManagement.Tests.csproj ShipManagement/tests/ShipManagement.Tests/
RUN dotnet restore ShipManagement/src/ShipManagement.API/ShipManagement.API.csproj

COPY ../../ShipManagement ShipManagement
RUN dotnet publish ShipManagement/src/ShipManagement.API/ShipManagement.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ShipManagement.API.dll"]
