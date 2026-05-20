# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Payment/PaymentService.API/PaymentService.API.csproj Payment/PaymentService.API/
COPY src/Payment/PaymentService.Application/PaymentService.Application.csproj Payment/PaymentService.Application/
COPY src/Payment/PaymentService.Domain/PaymentService.Domain.csproj Payment/PaymentService.Domain/
COPY src/Payment/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj Payment/PaymentService.Infrastructure/
COPY src/Payment/PaymentService.Contracts/PaymentService.Contracts.csproj Payment/PaymentService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Payment/PaymentService.API/PaymentService.API.csproj

COPY src/Payment/ ./Payment/
WORKDIR /src/Payment/PaymentService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Payment/ ./Payment/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Payment/PaymentService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
