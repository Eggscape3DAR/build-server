# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["BuildServer/BuildServer.csproj", "BuildServer/"]
RUN dotnet restore "BuildServer/BuildServer.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/BuildServer"
RUN dotnet build "BuildServer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BuildServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "BuildServer.dll"]
