FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BoltonCup.WebAPI/BoltonCup.WebAPI.csproj", "BoltonCup.WebAPI/"]
COPY ["BoltonCup.Core/BoltonCup.Core.csproj", "BoltonCup.Core/"]
COPY ["BoltonCup.Infrastructure/BoltonCup.Infrastructure.csproj", "BoltonCup.Infrastructure/"]
RUN dotnet restore "BoltonCup.WebAPI/BoltonCup.WebAPI.csproj"
COPY . .
WORKDIR "/src/BoltonCup.WebAPI"
RUN dotnet build "BoltonCup.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BoltonCup.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltonCup.WebAPI.dll"]
