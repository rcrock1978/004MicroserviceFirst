# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Customer/CustomerService.API/CustomerService.API.csproj Customer/CustomerService.API/
COPY src/Customer/CustomerService.Application/CustomerService.Application.csproj Customer/CustomerService.Application/
COPY src/Customer/CustomerService.Domain/CustomerService.Domain.csproj Customer/CustomerService.Domain/
COPY src/Customer/CustomerService.Infrastructure/CustomerService.Infrastructure.csproj Customer/CustomerService.Infrastructure/
COPY src/Customer/CustomerService.Contracts/CustomerService.Contracts.csproj Customer/CustomerService.Contracts/
COPY src/Shared/ ./Shared/

RUN dotnet restore Customer/CustomerService.API/CustomerService.API.csproj

COPY src/Customer/ ./Customer/
WORKDIR /src/Customer/CustomerService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrations
WORKDIR /src
COPY src/Customer/ ./Customer/
COPY src/Shared/ ./Shared/
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src/Customer/CustomerService.Infrastructure

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "CustomerService.API.dll"]
