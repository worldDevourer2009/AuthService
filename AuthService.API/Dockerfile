﻿FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AuthService.API/AuthService.API.csproj", "AuthService.API/"]

RUN --mount=type=secret,id=nuget_token \
    dotnet nuget add source "https://nuget.pkg.github.com/worldDevourer2009/index.json" \
      --name github \
      --username worldDevourer2009 \
      --password $(cat /run/secrets/nuget_token) \
      --store-password-in-clear-text

RUN dotnet restore "AuthService.API/AuthService.API.csproj"
COPY . .
WORKDIR "/src/AuthService.API"
RUN dotnet build "./AuthService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AuthService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]