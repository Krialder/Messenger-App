FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/FileTransferService/FileTransferService.csproj", "FileTransferService/"]
COPY ["src/Shared/MessengerContracts/MessengerContracts.csproj", "MessengerContracts/"]
COPY ["src/Shared/MessengerCommon/MessengerCommon.csproj", "MessengerCommon/"]
COPY ["src/Backend/CryptoService/CryptoService.csproj", "CryptoService/"]
RUN dotnet restore "FileTransferService/FileTransferService.csproj"

COPY src/Backend/FileTransferService/ FileTransferService/
COPY src/Shared/MessengerContracts/ MessengerContracts/
COPY src/Shared/MessengerCommon/ MessengerCommon/
COPY src/Backend/CryptoService/ CryptoService/

WORKDIR "/src/FileTransferService"
RUN dotnet build "FileTransferService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileTransferService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileTransferService.dll"]
