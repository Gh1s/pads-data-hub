﻿FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PadsDataHub/PadsDataHub.csproj", "PadsDataHub/"]
RUN dotnet restore "PadsDataHub/PadsDataHub.csproj"
COPY . .
WORKDIR "/src/PadsDataHub"
RUN dotnet build "PadsDataHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PadsDataHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PadsDataHub.dll"]
