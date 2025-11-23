# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de solução e restaura as dependências
COPY WorkWell.sln .
COPY WorkWell.API/*.csproj WorkWell.API/
COPY WorkWell.Application/*.csproj WorkWell.Application/
COPY WorkWell.Domain/*.csproj WorkWell.Domain/
COPY WorkWell.Infrastructure/*.csproj WorkWell.Infrastructure/
COPY WorkWell.Tests/*.csproj WorkWell.Tests/
RUN dotnet restore WorkWell.sln

# Copia o restante dos arquivos e faz o build
COPY . .
WORKDIR /src/WorkWell.API
RUN dotnet publish -c Release -o /app/publish

# Estágio Final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copia os arquivos publicados
COPY --from=build /app/publish .

# Define o ponto de entrada
ENTRYPOINT ["dotnet", "WorkWell.API.dll"]
