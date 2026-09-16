# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
COPY ["SistemaPastelaria.csproj", "./"]
RUN dotnet restore "SistemaPastelaria.csproj"

# Copia todo o resto do código e publica a aplicação
COPY . .
RUN dotnet publish "SistemaPastelaria.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final / Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# O Render define a porta dinamicamente através da variável de ambiente PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENTRYPOINT ["dotnet", "SistemaPastelaria.dll"]