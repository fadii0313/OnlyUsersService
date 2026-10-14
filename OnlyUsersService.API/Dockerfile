# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["OnlyUsersService.API/OnlyUsersService.API.csproj", "OnlyUsersService.API/"]
RUN dotnet restore "OnlyUsersService.API/OnlyUsersService.API.csproj"

# Copy full source and publish release build
COPY . .
WORKDIR "/src/OnlyUsersService.API"
RUN dotnet publish "OnlyUsersService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OnlyUsersService.API.dll"]
