#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LogCollector/LogCollectorAPI.csproj", "LogCollector/"]
COPY ["LogCollectorDOL/LogCollectorDOL.csproj", "LogCollectorDOL/"]

RUN dotnet restore "./LogCollectorDOL/./LogCollectorDOL.csproj"
RUN dotnet restore "./LogCollector/./LogCollectorAPI.csproj"

COPY LogCollector/. ./LogCollector
COPY LogCollectorDOL/. ./LogCollectorDOL

WORKDIR "/src/LogCollectorDOL"
RUN dotnet build "./LogCollectorDOL.csproj" -c $BUILD_CONFIGURATION -o /app/build

WORKDIR "/src/LogCollector"
RUN dotnet build "./LogCollectorAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LogCollectorAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogCollectorAPI.dll"]