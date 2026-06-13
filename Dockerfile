# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY Backend/*.csproj ./Backend/
RUN dotnet restore Backend/*.csproj

# Copy the rest of the source code and publish
COPY . .
RUN dotnet publish Backend/*.csproj -c Release -o /app

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Use the port provided by Render (fallback to 8080 for local testing)
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "E7jezli.Server.dll"]
