# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Order/OrderService.API/OrderService.API.csproj Order/OrderService.API/
COPY src/Order/OrderService.Application/OrderService.Application.csproj Order/OrderService.Application/
COPY src/Order/OrderService.Domain/OrderService.Domain.csproj Order/OrderService.Domain/
COPY src/Order/OrderService.Infrastructure/OrderService.Infrastructure.csproj Order/OrderService.Infrastructure/
COPY src/Order/OrderService.Contracts/OrderService.Contracts.csproj Order/OrderService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Order/OrderService.API/OrderService.API.csproj

COPY src/Order/ ./Order/
WORKDIR /src/Order/OrderService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Order/ ./Order/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Order/OrderService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "OrderService.API.dll"]
