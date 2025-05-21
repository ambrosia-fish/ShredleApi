FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy and publish app
COPY . .
RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Make the port binding explicit for Heroku - this reads from the same constant
# as the application code, but must use a hardcoded value in the Dockerfile
ENV PORT=5001
ENV ASPNETCORE_URLS=http://*:${PORT}

CMD dotnet shredle-api.dll
