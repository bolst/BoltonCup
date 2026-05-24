FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y python3 && apt-get clean && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/bin/python3 /usr/bin/python
WORKDIR /src
COPY ["BoltonCup.Auth/BoltonCup.Auth.csproj", "BoltonCup.Auth/"]
COPY ["BoltonCup.Common/BoltonCup.Common.csproj", "BoltonCup.Common/"]
RUN dotnet workload restore "BoltonCup.Auth/BoltonCup.Auth.csproj"
RUN dotnet restore "BoltonCup.Auth/BoltonCup.Auth.csproj"
COPY . .
WORKDIR "/src/BoltonCup.Auth"
RUN dotnet build "BoltonCup.Auth.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "BoltonCup.Auth.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
RUN apk add --no-cache openssl
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html
COPY BoltonCup.Auth/nginx.conf /etc/nginx/nginx.conf
COPY BoltonCup.Auth/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh
EXPOSE 443
ENTRYPOINT ["/entrypoint.sh"]
