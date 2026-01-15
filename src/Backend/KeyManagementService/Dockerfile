# ========================================
# PSEUDO-CODE - Sprint 6: KeyManagementService Dockerfile
# ========================================

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "Shared/MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "Shared/MessengerCommon/"]
COPY ["src/Backend/KeyManagementService/KeyManagementService.csproj", "Backend/KeyManagementService/"]

RUN dotnet restore "Backend/KeyManagementService/KeyManagementService.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Backend/KeyManagementService/", "Backend/KeyManagementService/"]

WORKDIR "/src/Backend/KeyManagementService"
RUN dotnet build "KeyManagementService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KeyManagementService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "KeyManagementService.dll"]
