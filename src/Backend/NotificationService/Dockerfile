FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "Shared/MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "Shared/MessengerCommon/"]
COPY ["src/Backend/NotificationService/NotificationService.csproj", "Backend/NotificationService/"]

RUN dotnet restore "Backend/NotificationService/NotificationService.csproj"

COPY ["src/Shared/", "Shared/"]
COPY ["src/Backend/NotificationService/", "Backend/NotificationService/"]

WORKDIR "/src/Backend/NotificationService"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "NotificationService.dll"]
