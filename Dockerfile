FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShredleApi.csproj", "."]
RUN dotnet restore "./ShredleApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ShredleApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShredleApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet ShredleApi.dll