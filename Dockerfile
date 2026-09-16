FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todo o código de uma vez para evitar erros de caminho
COPY . .
RUN dotnet restore "SistemaPastelaria.csproj"

RUN dotnet publish "SistemaPastelaria.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENTRYPOINT ["dotnet", "SistemaPastelaria.dll"]