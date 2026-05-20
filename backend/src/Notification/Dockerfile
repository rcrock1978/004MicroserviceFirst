# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Notification/NotificationService.API/NotificationService.API.csproj Notification/NotificationService.API/
COPY src/Notification/NotificationService.Application/NotificationService.Application.csproj Notification/NotificationService.Application/
COPY src/Notification/NotificationService.Domain/NotificationService.Domain.csproj Notification/NotificationService.Domain/
COPY src/Notification/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj Notification/NotificationService.Infrastructure/
COPY src/Notification/NotificationService.Contracts/NotificationService.Contracts.csproj Notification/NotificationService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Notification/NotificationService.API/NotificationService.API.csproj

COPY src/Notification/ ./Notification/
WORKDIR /src/Notification/NotificationService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Notification/ ./Notification/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Notification/NotificationService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
