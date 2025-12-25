# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Copy project file and restore dependencies (for better layer caching)
COPY *.csproj ./
RUN dotnet restore

# Copy remaining source files
COPY . ./

# Build and publish in one stage
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "OrderService.dll"]
EXPOSE 5000 5001
