# ========================================
# PSEUDO-CODE - Sprint 7: UserService Dockerfile
# ========================================

# UserService Dockerfile - .NET 9.0

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "Shared/MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "Shared/MessengerCommon/"]
COPY ["src/Backend/UserService/UserService.csproj", "Backend/UserService/"]

RUN dotnet restore "Backend/UserService/UserService.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Backend/UserService/", "Backend/UserService/"]

WORKDIR "/src/Backend/UserService"
RUN dotnet build "UserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "UserService.dll"]
