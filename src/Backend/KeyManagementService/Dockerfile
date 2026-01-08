# ========================================
# PSEUDO-CODE - Sprint 6: KeyManagementService Dockerfile
# ========================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/KeyManagementService/KeyManagementService.csproj", "KeyManagementService/"]
RUN dotnet restore "KeyManagementService/KeyManagementService.csproj"
COPY src/Backend/KeyManagementService/ KeyManagementService/
WORKDIR "/src/KeyManagementService"
RUN dotnet build "KeyManagementService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KeyManagementService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KeyManagementService.dll"]
