# AuthService Dockerfile - .NET 9.0 Multi-Stage Build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy shared projects first for better layer caching
COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "Shared/MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "Shared/MessengerCommon/"]

# Copy AuthService project file
COPY ["src/Backend/AuthService/AuthService.csproj", "Backend/AuthService/"]

# Restore dependencies
RUN dotnet restore "Backend/AuthService/AuthService.csproj"

# Copy source code
COPY ["src/Shared/", "Shared/"]
COPY ["src/Backend/AuthService/", "Backend/AuthService/"]

WORKDIR "/src/Backend/AuthService"
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "AuthService.dll"]
