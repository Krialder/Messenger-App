# CryptoService Dockerfile - .NET 9.0

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "Shared/MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "Shared/MessengerCommon/"]
COPY ["src/Backend/CryptoService/CryptoService.csproj", "Backend/CryptoService/"]

RUN dotnet restore "Backend/CryptoService/CryptoService.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Backend/CryptoService/", "Backend/CryptoService/"]

WORKDIR "/src/Backend/CryptoService"
RUN dotnet build "CryptoService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "CryptoService.dll"]
