FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BoltonCup.Admin/src/BoltonCup.Admin.csproj", "BoltonCup.Admin/src/"]
COPY ["BoltonCup.Common/src/BoltonCup.Common.csproj", "BoltonCup.Common/src/"]
COPY ["BoltonCup.Core/src/BoltonCup.Core.csproj", "BoltonCup.Core/src/"]
COPY ["BoltonCup.Infrastructure/BoltonCup.Infrastructure.csproj", "BoltonCup.Infrastructure/"]
RUN dotnet restore "BoltonCup.Admin/src/BoltonCup.Admin.csproj"
COPY . .
WORKDIR "/src/BoltonCup.Admin/src"
RUN dotnet build "./BoltonCup.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./BoltonCup.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltonCup.Admin.dll"]
