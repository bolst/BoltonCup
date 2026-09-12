FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BoltonCup.WebAPI/src/BoltonCup.WebAPI.csproj", "BoltonCup.WebAPI/src/"]
COPY ["BoltonCup.Core/src/BoltonCup.Core.csproj", "BoltonCup.Core/src/"]
COPY ["BoltonCup.Application/src/BoltonCup.Application.csproj", "BoltonCup.Application/src/"]
COPY ["BoltonCup.Shared/src/BoltonCup.Shared.csproj", "BoltonCup.Shared/src/"]
COPY ["BoltonCup.Integrations/src/BoltonCup.Integrations.csproj", "BoltonCup.Integrations/src/"]
COPY ["BoltonCup.Persistence/src/BoltonCup.Persistence.csproj", "BoltonCup.Persistence/src/"]
RUN dotnet restore "BoltonCup.WebAPI/src/BoltonCup.WebAPI.csproj"
COPY . .
WORKDIR "/src/BoltonCup.WebAPI/src"
RUN dotnet build "BoltonCup.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BoltonCup.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltonCup.WebAPI.dll"]
