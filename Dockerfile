FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /src
COPY ["DataManager.Web/DataManager.Web.csproj", "DataManager.Web/"]
RUN dotnet restore "DataManager.Web/DataManager.Web.csproj"
COPY . .
WORKDIR "/src/DataManager.Web"
RUN dotnet build "DataManager.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DataManager.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DataManager.Web.dll"]
