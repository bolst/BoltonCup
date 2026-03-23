FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BoltonCup.Admin/BoltonCup.Admin.csproj", "BoltonCup.Admin/"]
COPY ["BoltonCup.Common/BoltonCup.Common.csproj", "BoltonCup.Common/"]
COPY ["BoltonCup.Core/BoltonCup.Core.csproj", "BoltonCup.Core/"]
COPY ["BoltonCup.Infrastructure/BoltonCup.Infrastructure.csproj", "BoltonCup.Infrastructure/"]
RUN dotnet restore "BoltonCup.Admin/BoltonCup.Admin.csproj"
COPY . .
WORKDIR "/src/BoltonCup.Admin"
RUN dotnet build "./BoltonCup.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./BoltonCup.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BoltonCup.Admin.dll"]
