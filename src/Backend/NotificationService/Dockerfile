FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "MessengerCommon/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"

COPY src/Backend/NotificationService/ NotificationService/
COPY src/Shared/MessengerContracts/ MessengerContracts/
COPY src/Shared/MessengerCommon/ MessengerCommon/

WORKDIR "/src/NotificationService"
RUN dotnet build "NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
