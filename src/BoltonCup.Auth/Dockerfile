FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BoltonCup.Auth/BoltonCup.Auth.csproj", "BoltonCup.Auth/"]
COPY ["BoltonCup.Common/BoltonCup.Common.csproj", "BoltonCup.Common/"]
RUN dotnet restore "BoltonCup.Auth/BoltonCup.Auth.csproj"
COPY . .
WORKDIR "/src/BoltonCup.Auth"
RUN dotnet build "BoltonCup.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoltonCup.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltonCup.Auth.dll"]
