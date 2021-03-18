#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/WebNavigator/WebNavigator.csproj", "src/WebNavigator/"]
RUN dotnet restore "src/WebNavigator/WebNavigator.csproj"
COPY . .
WORKDIR "/src/src/WebNavigator"
RUN dotnet build "WebNavigator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebNavigator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebNavigator.dll"]