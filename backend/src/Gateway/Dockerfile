# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY src/Gateway/Gateway.API/Gateway.API.csproj Gateway/Gateway.API/
COPY src/Shared/ ./Shared/

RUN dotnet restore Gateway/Gateway.API/Gateway.API.csproj

# Copy source and build
COPY src/Gateway/ ./Gateway/
WORKDIR /src/Gateway/Gateway.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
USER $APP_UID

# Read-only root filesystem where possible
# Note: ASP.NET may need /tmp for some operations

COPY --from=build /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Gateway.API.dll"]
