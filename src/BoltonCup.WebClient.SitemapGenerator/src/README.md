# Bolton Cup Sitemap Renderer

> Credit to [TehGM/Randominator](https://github.com/TehGM/Randominator) for the source of this generator.

Use to generate sitemap.xml file before deployment (e.g., GitHub Actions).

## Usage
Use `dotnet run` to run this after [BoltonCup.WebClient](../BoltonCup.WebClient/BoltonCup.WebClient.csproj) has been published.  

Options:
- `-o`: controls output directory (default is `-o bin/publish/wwwroot`).
- `-f`: controls file name (default is `-f sitemap.xml`).
- `-d`: allows changing domain (default is `-d randominator.tehgm.net`).
- `-p`: specifies procotol/scheme (default is `-p https`).
- `--debug`: enables debug logs.