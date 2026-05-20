# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY src/Identity/IdentityService.API/IdentityService.API.csproj Identity/IdentityService.API/
COPY src/Identity/IdentityService.Application/IdentityService.Application.csproj Identity/IdentityService.Application/
COPY src/Identity/IdentityService.Domain/IdentityService.Domain.csproj Identity/IdentityService.Domain/
COPY src/Identity/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj Identity/IdentityService.Infrastructure/
COPY src/Identity/IdentityService.Contracts/IdentityService.Contracts.csproj Identity/IdentityService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Identity/IdentityService.API/IdentityService.API.csproj

# Copy source and build
COPY src/Identity/ ./Identity/
WORKDIR /src/Identity/IdentityService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Migration tool stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Identity/ ./Identity/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Identity/IdentityService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "IdentityService.API.dll"]
