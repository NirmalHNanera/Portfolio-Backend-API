FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PortfolioContactAPI.csproj", "./"]
RUN dotnet restore "./PortfolioContactAPI.csproj"
COPY . .
RUN dotnet publish "PortfolioContactAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "PortfolioContactAPI.dll"]