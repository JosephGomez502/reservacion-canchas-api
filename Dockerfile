# =====================================================
# ETAPA 1 - IMAGEN BASE
# =====================================================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER app

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

# =====================================================
# ETAPA 2 - COMPILACIÓN
# =====================================================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["Canchas.Api/Canchas.Api.csproj", "Canchas.Api/"]

RUN dotnet restore "Canchas.Api/Canchas.Api.csproj"

COPY . .

WORKDIR "/src/Canchas.Api"

RUN dotnet build "Canchas.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# =====================================================
# ETAPA 3 - PUBLICACIÓN
# =====================================================

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Canchas.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# =====================================================
# ETAPA 4 - IMAGEN FINAL
# =====================================================

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Canchas.Api.dll"]