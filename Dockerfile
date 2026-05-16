# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore
COPY Backend/*.csproj ./Backend/
RUN dotnet restore Backend/*.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish Backend/*.csproj -c Release -o /app

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Expose the port Render expects
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "E7jezli.Server.dll"]
