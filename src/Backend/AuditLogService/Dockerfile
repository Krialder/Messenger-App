# AuditLogService Dockerfile - .NET 8.0
# Status: ✅ PRODUCTION (v10.1)

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "Shared/MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "Shared/MessengerCommon/"]
COPY ["src/Backend/AuditLogService/AuditLogService.csproj", "Backend/AuditLogService/"]

RUN dotnet restore "Backend/AuditLogService/AuditLogService.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Backend/AuditLogService/", "Backend/AuditLogService/"]

WORKDIR "/src/Backend/AuditLogService"
RUN dotnet build "AuditLogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuditLogService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "AuditLogService.dll"]
