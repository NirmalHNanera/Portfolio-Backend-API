# Use official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PortfolioContactAPI.csproj", "./"]
RUN dotnet restore "./PortfolioContactAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "PortfolioContactAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage: build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port Render expects (usually 80 or 8080, we will use 80)
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "PortfolioContactAPI.dll"]
