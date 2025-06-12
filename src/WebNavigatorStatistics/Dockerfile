#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
# https://github.com/dotnet/dotnet-docker/blob/main/documentation/azurelinux.md
FROM mcr.microsoft.com/dotnet/aspnet:9.0-azurelinux3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-azurelinux3.0 AS build
WORKDIR /src/WebNavigatorStatistics
COPY src/WebNavigatorStatistics/WebNavigatorStatistics.csproj .
RUN dotnet restore WebNavigatorStatistics.csproj
COPY . .
RUN dotnet build "WebNavigatorStatistics.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebNavigatorStatistics.csproj" -c Release -o /app/publish

ENV ASPNETCORE_URLS http://*:8080
EXPOSE 8080

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebNavigatorStatistics.dll"]
