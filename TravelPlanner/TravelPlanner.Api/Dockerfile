# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Buid Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better caching
COPY ["TravelPlanner.Api/TravelPlanner.Api.csproj", "TravelPlanner.Api/"]
COPY ["TravelPlanner.Infrastructure/TravelPlanner.Infrastructure.csproj", "TravelPlanner.Infrastructure/"]
COPY ["TravelPlanner.Application/TravelPlanner.Application.csproj", "TravelPlanner.Application/"]
COPY ["TravelPlanner.Domain/TravelPlanner.Domain.csproj", "TravelPlanner.Domain/"]
RUN dotnet restore "TravelPlanner.Api/TravelPlanner.Api.csproj"

# Copy rest of the files and publish
COPY . .
RUN dotnet publish "TravelPlanner.Api/TravelPlanner.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TravelPlanner.Api.dll"]