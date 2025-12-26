# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file and restore dependencies (build context is repo root)
COPY ["OrderService/OrderService.csproj", "OrderService/"]
RUN dotnet restore "OrderService/OrderService.csproj"

# Copy remaining source files
COPY OrderService/ OrderService/

# Build and publish
WORKDIR /src/OrderService
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Run the application (PORT is set by Railway at runtime)
ENTRYPOINT ["dotnet", "OrderService.dll"]
