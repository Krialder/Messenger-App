# ========================================
# PSEUDO-CODE - Sprint 11: AuditLogService Dockerfile
# ========================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Backend/AuditLogService/AuditLogService.csproj", "AuditLogService/"]
RUN dotnet restore "AuditLogService/AuditLogService.csproj"
COPY src/Backend/AuditLogService/ AuditLogService/
WORKDIR "/src/AuditLogService"
RUN dotnet build "AuditLogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuditLogService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuditLogService.dll"]
