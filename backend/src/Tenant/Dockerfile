# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Tenant/TenantService.API/TenantService.API.csproj Tenant/TenantService.API/
COPY src/Tenant/TenantService.Application/TenantService.Application.csproj Tenant/TenantService.Application/
COPY src/Tenant/TenantService.Domain/TenantService.Domain.csproj Tenant/TenantService.Domain/
COPY src/Tenant/TenantService.Infrastructure/TenantService.Infrastructure.csproj Tenant/TenantService.Infrastructure/
COPY src/Tenant/TenantService.Contracts/TenantService.Contracts.csproj Tenant/TenantService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Tenant/TenantService.API/TenantService.API.csproj

COPY src/Tenant/ ./Tenant/
WORKDIR /src/Tenant/TenantService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Tenant/ ./Tenant/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Tenant/TenantService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "TenantService.API.dll"]
