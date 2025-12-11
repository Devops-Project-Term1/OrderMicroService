# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Copy project files
COPY ["OrderService.csproj", "./"]
COPY ["Controllers/", "./Controllers/"]
COPY ["data/", "./data/"]
COPY ["Migrations/", "./Migrations/"]
COPY ["Model/", "./Model/"]
COPY ["Service/", "./Service/"]
COPY ["Properties/", "./Properties/"]
COPY ["Program.cs", "./"]
COPY ["appsettings.json", "./"]
COPY ["appsettings.Development.json", "./"]

# Restore and build
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "OrderService.dll"]
EXPOSE 5000 5001
