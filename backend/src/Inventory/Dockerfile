# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Inventory/InventoryService.API/InventoryService.API.csproj Inventory/InventoryService.API/
COPY src/Inventory/InventoryService.Application/InventoryService.Application.csproj Inventory/InventoryService.Application/
COPY src/Inventory/InventoryService.Domain/InventoryService.Domain.csproj Inventory/InventoryService.Domain/
COPY src/Inventory/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj Inventory/InventoryService.Infrastructure/
COPY src/Inventory/InventoryService.Contracts/InventoryService.Contracts.csproj Inventory/InventoryService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Inventory/InventoryService.API/InventoryService.API.csproj

COPY src/Inventory/ ./Inventory/
WORKDIR /src/Inventory/InventoryService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Inventory/ ./Inventory/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Inventory/InventoryService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "InventoryService.API.dll"]
